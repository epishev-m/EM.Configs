using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EM.Foundation;

namespace EM.Configs
{

public sealed class Validator : IValidator
{
	private readonly ICustomValidator[] _validators;

	private List<ValidationResult> _validationResults;

	private bool _operationResult;

	#region IValidator

	public bool TryValidate(object instance,
		List<ValidationResult> validationResults)
	{
		Requires.NotNull(instance, nameof(instance));
		Requires.NotNull(validationResults, nameof(validationResults));

		Cleanup();
		_validationResults = validationResults;
		EnumerationOfFields(instance, instance.GetType().Name);

		return _operationResult;
	}

	#endregion

	#region Validator

	public Validator(params ICustomValidator[] validators)
	{
		_validators = validators;
	}

	private void Cleanup()
	{
		_validationResults = null;
		_operationResult = true;
	}

	private void EnumerationOfFields(object instance,
		string path)
	{
		if (instance == null)
		{
			return;
		}

		var type = instance.GetType();
		var fields = type.GetFields();

		foreach (var field in fields)
		{
			var fieldValue = field.GetValue(instance);
			var fieldType = field.FieldType;
			var fieldName = GetFieldName(fieldValue, field.Name);
			Validate(field, fieldValue, fieldName, path);
			AttributeValidate(field, fieldValue, fieldName, path);

			if (CheckIsCollection(fieldValue, fieldName, path))
			{
				continue;
			}

			CheckIsClass(fieldValue, fieldType, fieldName, path);
		}
	}

	private static string GetFieldName(object value,
		string name)
	{
		if (value is ILibraryEntry libraryEntry)
		{
			return $"{name} {{Id : \"{libraryEntry.Id}\"}}";
		}

		return name;
	}

	private void Validate(FieldInfo field,
		object value,
		string name,
		string parentPath)
	{
		foreach (var validator in _validators)
		{
			if (validator.TryValidate(field, value, out var errorMessage))
			{
				continue;
			}

			_operationResult = false;
			var path = $"{parentPath}.{name}";
			AddError(validator.ValidationType, path, errorMessage);
		}
	}

	private void AttributeValidate(FieldInfo field,
		object value,
		string name,
		string parentPath)
	{
		var attributes = field.GetCustomAttributes<ValidatorAttribute>();

		foreach (var validator in attributes)
		{
			if (validator.TryValidate(field, value, out var errorMessage))
			{
				continue;
			}

			_operationResult = false;
			var path = $"{parentPath}.{name}";
			AddError(validator.ValidationType, path, errorMessage);
		}
	}

	private void AddError(string validationType,
		string path,
		string errorMessage)
	{
		var result = _validationResults.FirstOrDefault(result => result.ValidationType == validationType);

		if (result == null)
		{
			result = new ValidationResult(validationType);
			_validationResults.Add(result);
		}

		result.AddResult(path, errorMessage);
	}

	private bool CheckIsCollection(object value,
		string name,
		string parentPath)
	{
		if (value is not ICollection collection)
		{
			return false;
		}

		var index = 0;

		foreach (var item in collection)
		{
			if (item == null)
			{
				continue;
			}

			var itemName = GetCollectionItemName(item, index);
			var path = $"{parentPath}.{name}[] {itemName}";
			EnumerationOfFields(item, path);
			index++;
		}

		return true;
	}

	private static string GetCollectionItemName(object item, int index)
	{
		if (item is ILibraryEntry libraryEntry)
		{
			return $"{{Index: {index}, Id: \"{libraryEntry.Id}\"}}";
		}

		return $"{{Index: {index}}}";
	}

	private void CheckIsClass(object value,
		Type type,
		string name,
		string parentPath)
	{
		if (!type.IsClass)
		{
			return;
		}

		if (value is string)
		{
			return;
		}

		if (value is BaseLibraryEntryLink)
		{
			return;
		}

		var path = $"{parentPath}.{name}";
		EnumerationOfFields(value, path);
	}

	#endregion

}

}