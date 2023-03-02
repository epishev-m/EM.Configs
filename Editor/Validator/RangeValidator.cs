namespace EM.Configs.Editor
{

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class RangeValidator : IConfigsValidator
{
	private readonly StringBuilder _errorMessage = new();

	private readonly List<string> _errors = new();

	#region IConfigsValidator

	public string ErrorMassage => _errorMessage.ToString();

	public bool Validate(object config)
	{
		Requires.NotNull(config, nameof(config));

		Clear();
		FillErrors(config, config.GetType().Name);
		GenerateErrorLog();

		return !_errors.Any();
	}

	#endregion

	#region RangeValidator

	private void Clear()
	{
		_errors.Clear();
		_errorMessage.Clear();
	}

	private void FillErrors(object instance,
		string path)
	{
		var type = instance.GetType();
		var fields = type.GetFields();

		foreach (var field in fields)
		{
			var fieldValue = field.GetValue(instance);
			var fieldType = field.FieldType;

			if (fieldValue == null)
			{
				continue;
			}

			if (CheckExcludedClasses(fieldValue))
			{
				continue;
			}

			if (CheckIsCollection(fieldValue, field.Name, path))
			{
				continue;
			}

			if (CheckRange(field, fieldValue, fieldType, instance, path))
			{
				continue;
			}

			if (fieldType.IsClass)
			{
				FillErrors(fieldValue, $"{path}.{field.Name}");
			}
		}
	}

	private void GenerateErrorLog()
	{
		if (_errors.Any())
		{
			_errorMessage.AppendLine($"{nameof(RangeValidator)} :: Value is out of range");
		}

		foreach (var error in _errors)
		{
			_errorMessage.AppendLine($" - {error}");
		}
	}

	private static bool CheckExcludedClasses(object fieldValue)
	{
		if (fieldValue is string)
		{
			return true;
		}

		if (fieldValue is ConfigLink)
		{
			return true;
		}

		return false;
	}

	private bool CheckIsCollection(object fieldValue,
		string name,
		string path)
	{
		if (fieldValue is not ICollection collection)
		{
			return false;
		}

		foreach (var obj in collection)
		{
			if (obj == null)
			{
				continue;
			}

			FillErrors(obj, $"{path}.{name}[]");
		}

		return true;
	}

	private bool CheckRange(FieldInfo fieldInfo,
		object fieldValue,
		Type fieldType,
		object parent,
		string path)
	{
		if (!fieldType.IsPrimitive)
		{
			return true;
		}

		if (fieldType == typeof(int))
		{
			var value = (int) fieldValue;
			var range = fieldInfo.GetCustomAttribute<RangeIntAttribute>();
			CheckRangeError(fieldInfo, range, value, parent, path);

			return true;
		}

		if (fieldType == typeof(long))
		{
			var value = (long) fieldValue;
			var range = fieldInfo.GetCustomAttribute<RangeLongAttribute>();
			CheckRangeError(fieldInfo, range, value, parent, path);

			return true;
		}

		if (fieldType == typeof(float))
		{
			var value = (float) fieldValue;
			var range = fieldInfo.GetCustomAttribute<RangeFloatAttribute>();
			CheckRangeError(fieldInfo, range, value, parent, path);

			return true;
		}

		if (fieldType == typeof(double))
		{
			var value = (double) fieldValue;
			var range = fieldInfo.GetCustomAttribute<RangeDoubleAttribute>();
			CheckRangeError(fieldInfo, range, value, parent, path);

			return true;
		}

		return false;
	}

	private void CheckRangeError<T>(FieldInfo fieldInfo,
		IRangeAttribute<T> range,
		T value,
		object parent,
		string path)
		where T : struct, IComparable
	{
		if (range == null)
		{
			return;
		}

		if (range.Check(value))
		{
			return;
		}

		var parentInfo = GetParentInfo(parent);
		_errors.Add($"{parentInfo} / Path: {path}.{fieldInfo.Name} - " + 
					$"Value = {value} is out of range [{range.Min}:{range.Max}]");
	}

	private static string GetParentInfo(object parent)
	{
		var type = parent.GetType();
		var id = "##";
		var fieldInfoId = type.GetField("Id");

		if (fieldInfoId != null)
		{
			id = (string) fieldInfoId.GetValue(parent);
		}

		var result = $"Type: {parent.GetType().Name} / Id: {id}";

		return result;
	}

	#endregion
}

}