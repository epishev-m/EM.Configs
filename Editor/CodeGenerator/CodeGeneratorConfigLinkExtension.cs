namespace EM.Configs.Editor
{

using System;
using System.Collections.Generic;
using System.Reflection;
using Foundation.Editor;

public sealed class CodeGeneratorConfigLinkExtension : ICodeGenerator
{
	private const string UnwrapTemplate =
		"\n\tpublic static {0} Unwrap(this ConfigLink<{0}> configLink," + 
		"\n\t\tGameConfigs gameConfigs)" +
		"\n\t{{\n\t\tif (configLink.Value != null)" +
		"\n\t\t{{\n\t\t\treturn configLink.Value;\n\t\t}}" +
		"\n{1}\n\t\treturn null;\n\t}}\n";

	private const string contentUnwrapTemplate =
		"\n\t\tvar result{0} = gameConfigs.{0}.FirstOrDefault(item => item.Id == configLink.Id);" +
		"\n\n\t\tif (result{0} != null)\n\t\t{{\n\t\t\tconfigLink.Value = result{0};\n\n\t\t\treturn result{0};\n\t\t}}\n";

	private readonly TypeInfo _typeInfo;

	private readonly Dictionary<Type, List<string>> _fields = new();

	#region ICodeGenerator

	public string Create()
	{
		FillFields();
		var code = GenerateCode();

		return code;
	}

	#endregion

	#region CodeGeneratorConfigLinkExtension

	public CodeGeneratorConfigLinkExtension(TypeInfo typeInfo)
	{
		_typeInfo = typeInfo;
	}

	private string GenerateCode()
	{
		var code = string.Empty;
		
		foreach (var (type, namesList) in _fields)
		{
			var contentUnwrap = string.Empty;

			foreach (var name in namesList)
			{
				contentUnwrap += string.Format(contentUnwrapTemplate, name);
			}

			code += string.Format(UnwrapTemplate, type, contentUnwrap);
		}

		return code;
	}

	private void FillFields()
	{
		_fields.Clear();
		var fields = _typeInfo.GetFields();

		foreach (var fieldInfo in fields)
		{
			if (TryGetElementType(fieldInfo, out var elementType))
			{
				AddField(elementType, fieldInfo);
			}
		}
	}

	private static bool TryGetElementType(FieldInfo fieldInfo, out Type elementType)
	{
		var type = fieldInfo.FieldType;

		if (!type.IsArray)
		{
			elementType = null;

			return false;
		}

		elementType = type.GetElementType();

		if (elementType == null)
		{
			return false;
		}

		if (elementType.GetField("Id") == null)
		{
			elementType = null;

			return false;
		}

		return true;
	}

	private void AddField(Type type, FieldInfo fieldInfo)
	{
		var fieldName = fieldInfo.Name;

		if (!_fields.ContainsKey(type))
		{
			_fields.Add(type, new List<string>());
		}

		_fields[type].Add(fieldName);
	}

	#endregion
}

}