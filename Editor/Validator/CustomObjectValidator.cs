namespace EM.Configs.Editor
{

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class CustomObjectValidator : IConfigsValidator
{
	private readonly StringBuilder _errorMessage = new();

	private readonly Dictionary<Type, string> _errors = new();

	#region IConfigsValidator

	public string ErrorMassage { get; }

	public bool Validate(object config)
	{
		Requires.NotNull(config, nameof(config));

		Clear();
		FillErrors(config, config.GetType().Name);
		GenerateErrorLog();

		return !_errors.Any();
	}

	#endregion

	#region CustomObjectValidator

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

			CheckIsCustom(field, fieldValue, fieldType, instance, path);
		}
	}

	private void GenerateErrorLog()
	{
		if (_errors.Any())
		{
			_errorMessage.AppendLine($"{nameof(CustomObjectValidator)}");
		}

		foreach (var error in _errors)
		{
			_errorMessage.AppendLine($" - {error.Key} :: {error.Value}");
		}
	}

	private void CheckIsCustom(FieldInfo fieldInfo,
		object fieldValue,
		Type fieldType,
		object parent,
		string path)
	{
		if (!fieldType.IsClass)
		{
			return;
		}

		var validateWith = fieldInfo.GetCustomAttribute<ValidateWithAttribute>();

		if (validateWith == null)
		{
			FillErrors(fieldValue, $"{path}.{fieldInfo.Name}");

			return;
		}

		var validator = Activator.CreateInstance(validateWith.Type) as IConfigsValidator;

		Requires.NotNull(validator, nameof(validator));

		if (validator == null)
		{
			return;
		}

		if (!validator.Validate(parent))
		{
			var parentInfo = GetParentInfo(parent);
			var error = $"{parentInfo} / Path: {path}.{fieldInfo.Name} - {validator.ErrorMassage}";
			_errors.Add(fieldType, error);
		}
	}

	private static bool CheckExcludedClasses(object fieldValue)
	{
		if (fieldValue is string)
		{
			return true;
		}

		if (fieldValue is LinkDefinition)
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