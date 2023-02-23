namespace EM.Configs.Editor
{

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Foundation;

public sealed class ConfigLinkValidator : IConfigsValidator
{
	private readonly Dictionary<Type, List<ConfigLink>> _dictionaryLinks = new();

	private readonly Dictionary<Type, List<object>> _dictionaryObjects = new();

	private readonly StringBuilder _errorMessage = new();

	#region IConfigsValidator

	public string ErrorMassage => _errorMessage.ToString();

	public bool Validate(object config)
	{
		Requires.NotNull(config, nameof(config));

		Clear();
		FillLinks(config);
		FillObjects(config);
		GenerateErrorLog();

		return string.IsNullOrWhiteSpace(_errorMessage.ToString());
	}

	#endregion

	#region ConfigLinkValidator

	private void Clear()
	{
		_dictionaryLinks.Clear();
		_dictionaryObjects.Clear();
		_errorMessage.Clear();
	}

	private void FillLinks(object instance)
	{
		var type = instance.GetType();
		var fields = type.GetFields();

		foreach (var field in fields)
		{
			var fieldValue = field.GetValue(instance);
			var fieldType = field.FieldType;

			if (!CheckValueAndType(fieldValue, fieldType))
			{
				continue;
			}

			if (CheckIfTypeIsArrayLinks(fieldValue, fieldType))
			{
				continue;
			}

			if (!TryAddConfigLink(fieldValue, fieldType))
			{
				FillLinks(fieldValue);
			}
		}
	}

	private static bool CheckValueAndType(object fieldValue,
		Type fieldType)
	{
		switch (fieldValue)
		{
			case null:
			case string:
				return false;
		}

		return fieldType.IsClass;
	}

	private bool CheckIfTypeIsArrayLinks(object fieldValue,
		Type fieldType)
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

			var type = obj.GetType();

			if (TryAddConfigLink(obj, type))
			{
				continue;
			}

			if (CheckValueAndType(obj, type))
			{
				FillLinks(obj);
			}
		}

		return true;
	}

	private bool TryAddConfigLink(object fieldValue,
		Type fieldType)
	{
		if (!typeof(ConfigLink).IsAssignableFrom(fieldType))
		{
			return false;
		}

		var configLink = (ConfigLink) fieldValue;

		if (!_dictionaryLinks.ContainsKey(configLink.Type))
		{
			_dictionaryLinks.Add(configLink.Type, new List<ConfigLink>());
		}

		_dictionaryLinks[configLink.Type].Add(configLink);

		return true;
	}

	private void FillObjects(object instance)
	{
		var type = instance.GetType();
		var fields = type.GetFields();

		foreach (var field in fields)
		{
			var fieldValue = field.GetValue(instance);
			var fieldType = field.FieldType;

			if (!CheckValueAndType(fieldValue, fieldType))
			{
				continue;
			}

			if (CheckIfTypeIsArrayObjects(fieldValue, fieldType))
			{
				continue;
			}

			if (TryAddConfigObject(fieldValue, fieldType))
			{
				FillObjects(fieldValue);
			}
		}
	}

	private bool CheckIfTypeIsArrayObjects(object fieldValue,
		Type fieldType)
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

			var type = obj.GetType();

			if (TryAddConfigObject(obj, type))
			{
				continue;
			}

			if (CheckValueAndType(obj, type))
			{
				FillObjects(obj);
			}
		}

		return true;
	}

	private bool TryAddConfigObject(object fieldValue,
		Type fieldType)
	{
		if (!_dictionaryLinks.ContainsKey(fieldType))
		{
			return false;
		}

		if (!_dictionaryObjects.ContainsKey(fieldType))
		{
			_dictionaryObjects.Add(fieldType, new List<object>());
		}

		_dictionaryObjects[fieldType].Add(fieldValue);

		return true;
	}

	private void GenerateErrorLog()
	{
		foreach (var (type, configLinkList) in _dictionaryLinks)
		{
			if (CheckKeyAndFillErrors(type, configLinkList))
			{
				continue;
			}

			FillErrors(type, configLinkList);
		}
	}

	private object GetObjectById(ConfigLink configLink,
		FieldInfo fieldInfo)
	{
		var obj = _dictionaryObjects[configLink.Type].FirstOrDefault(obj =>
		{
			var id = (string) fieldInfo.GetValue(obj);

			return configLink.Id == id;
		});

		return obj;
	}

	private bool CheckKeyAndFillErrors(Type type,
		IEnumerable<ConfigLink> configLinkList)
	{
		if (_dictionaryObjects.ContainsKey(type))
		{
			return false;
		}

		foreach (var configLink in configLinkList)
		{
			AddLogError(configLink);
		}

		return true;
	}

	private void FillErrors(Type type,
		IEnumerable<ConfigLink> configLinkList)
	{
		var fieldInfo = type.GetField("Id");

		foreach (var configLink in configLinkList)
		{
			var obj = GetObjectById(configLink, fieldInfo);

			if (obj == null)
			{
				AddLogError(configLink);
			}
		}
	}

	private void AddLogError(ConfigLink configLink)
	{
		if (string.IsNullOrWhiteSpace(_errorMessage.ToString()))
		{
			_errorMessage.AppendLine($"{nameof(ConfigLinkValidator)} :: Not found objects:");
		}

		_errorMessage.AppendLine($" - Type: \"{configLink.Type.Name}\", Id: \"{configLink.Id}\"");
	}

	#endregion
}

}