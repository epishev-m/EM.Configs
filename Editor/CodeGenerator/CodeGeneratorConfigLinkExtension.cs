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
		"\n\t\tvar result{0} = gameConfigs.{0}.FirstOrDefault(item => item.Name == configLink.Name);" +
		"\n\n\t\tif (result{0} != null)\n\t\t{{\n\t\t\tconfigLink.Value = result{0};\n\n\t\t\treturn result{0};\n\t\t}}\n";

	private readonly TypeInfo _typeInfo;

	#region ICodeGenerator

	public string Create()
	{
		var code = string.Empty;
		var fields = GetFields();

		foreach (var (type, names) in fields)
		{
			var contentUnwrap = string.Empty;

			foreach (var name in names)
			{
				contentUnwrap += string.Format(contentUnwrapTemplate, name);
			}

			code += string.Format(UnwrapTemplate, type, contentUnwrap);
		}

		return code;
	}

	#endregion

	#region CodeGeneratorConfigLinkExtension

	public CodeGeneratorConfigLinkExtension(TypeInfo typeInfo)
	{
		_typeInfo = typeInfo;
	}

	private Dictionary<Type, List<string>> GetFields()
	{
		var temp = new Dictionary<Type, List<string>>();
		var fields = _typeInfo.GetFields();

		foreach (var fieldInfo in fields)
		{
			var type = fieldInfo.FieldType;

			if (!type.IsArray)
			{
				continue;
			}

			var elementType = type.GetElementType();

			if (elementType == null)
			{
				continue;
			}

			var fieldName = fieldInfo.Name;

			if (!temp.ContainsKey(elementType))
			{
				temp.Add(elementType, new List<string>());
			}

			temp[elementType].Add(fieldName);
		}

		return temp;
	}

	#endregion
}

}