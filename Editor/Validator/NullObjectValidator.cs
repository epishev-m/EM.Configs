namespace EM.Configs.Editor
{

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class NullObjectValidator : IConfigsValidator
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

	#region NullObjectValidator

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

			if (CheckObjectIsNull(field, fieldValue, instance, path))
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
			_errorMessage.AppendLine($"{nameof(NullObjectValidator)} :: Found empty fields");
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

	private bool CheckObjectIsNull(FieldInfo fieldInfo,
		object fieldValue,
		object parent,
		string path)
	{
		if (fieldValue != null)
		{
			return false;
		}

		var optionalField = fieldInfo.GetCustomAttribute<OptionalFieldAttribute>();

		if (optionalField != null)
		{
			return true;
		}

		var parentInfo = GetParentInfo(parent);
		_errors.Add($"{parentInfo} / Path: {path}.{fieldInfo.Name}");

		return true;
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
				var parentInfo = GetParentInfo(fieldValue);
				_errors.Add($"{parentInfo} / Path: {path}.{name}[] - Contains the null element");

				continue;
			}

			FillErrors(obj, $"{path}.{name}[]");
		}

		return true;
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