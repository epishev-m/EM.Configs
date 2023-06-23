namespace EM.Configs.Editor
{

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class CollectionSizeValidator : IConfigsValidator
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

	#region LengthValidator

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

			if (CheckIsCollection(field, fieldValue, field.Name, instance, path))
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
			_errorMessage.AppendLine($"{nameof(CollectionSizeValidator)} :: Collection size does not meet requirements");
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

		if (fieldValue is LinkConfig)
		{
			return true;
		}

		return false;
	}

	private bool CheckIsCollection(FieldInfo fieldInfo,
		object fieldValue,
		string name,
		object parent,
		string path)
	{
		if (fieldValue is not ICollection collection)
		{
			return false;
		}

		var range = fieldInfo.GetCustomAttribute<CollectionSizeAttribute>();

		if (range != null)
		{
			var length = collection.Count;

			if (length < range.Min || length > range.Max)
			{
				var parentInfo = GetParentInfo(parent);
				_errors.Add($"{parentInfo} / Path: {path}.{name}[] " +
							$"- Collection size = {length} is out of range [{range.Min}:{range.Max}]");
			}
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