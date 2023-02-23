namespace EM.Configs.Editor
{

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class StringEmptyValidator : IConfigsValidator
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

	#region StringEmptyValidator

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

			if (CheckString(field, fieldValue, instance, path))
			{
				continue;
			}

			if (CheckExcludedClasses(fieldValue))
			{
				continue;
			}

			if (CheckIsArray(fieldValue, fieldType, field.Name, path))
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
			_errorMessage.AppendLine($"{nameof(StringEmptyValidator)} :: Found empty strings");
		}

		foreach (var error in _errors)
		{
			_errorMessage.AppendLine($" - {error}");
		}
	}

	private bool CheckString(FieldInfo fieldInfo,
		object fieldValue,
		object parent,
		string path)
	{
		if (fieldValue is not string value)
		{
			return false;
		}

		if (!string.IsNullOrWhiteSpace(value))
		{
			return true;
		}

		var optionalField = fieldInfo.GetCustomAttribute<EmptyStringAllowedAttribute>();

		if (optionalField != null)
		{
			return true;
		}

		var parentInfo = GetParentInfo(parent);
		_errors.Add($"{parentInfo} / Path: {path}.{fieldInfo.Name}");

		return true;
	}

	private static bool CheckExcludedClasses(object fieldValue)
	{
		return fieldValue is ConfigLink;
	}

	private bool CheckIsArray(object fieldValue,
		Type fieldType,
		string name,
		string path)
	{
		if (!fieldType.IsArray)
		{
			return false;
		}

		foreach (var obj in (Array) fieldValue)
		{
			if (obj == null)
			{
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