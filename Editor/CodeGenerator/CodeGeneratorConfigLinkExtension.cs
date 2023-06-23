namespace EM.Configs.Editor
{

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant.Editor;

public sealed class CodeGeneratorConfigLinkExtension : ICodeGenerator
{
	private const string DefinitionTemplate = "\n\tpublic static {0} Definition;" + 
		"\n\n\tpublic static void SetConfig({0} definition)" +
		"\n\t{{\n\t\tif (Definition != null)" +
		"\n\t\t{{\n\t\t\treturn;\n\t\t}}" +
		"\n\n\t\tDefinition = definition;\n\t}}\n";

	private const string UnwrapTemplate =
		"\n\tpublic static {0} Unwrap(this LinkConfig<{0}> linkDefinition)" +
		"\n\t{{\n\t\tif (linkDefinition.Value != null)" +
		"\n\t\t{{\n\t\t\treturn linkDefinition.Value;\n\t\t}}" +
		"\n\n\t\tvar all = linkDefinition.GetAll();" +
		"\n\t\tlinkDefinition.Value = all.FirstOrDefault(item => item.{1} == linkDefinition.Id);" +
		"\n\n\t\treturn linkDefinition.Value;\n\t}}\n";

	private const string GetAllTemplate =
		"\n\tprivate static IEnumerable<{0}> GetAll(this LinkConfig<{0}> linkDefinition)" +
		"\n\t{{\n\t\tvar resultList = new List<{0}>();" +
		"{1}\n" +
		"\n\t\treturn resultList;\n\t}}\n";
	
	private const string ContentGetAllTemplate =
		"\n\n\t\tif (Definition{0} != null)" +
		"\n\t\t{{\n\t\t\tresultList.AddRange(Definition{0});\n\t\t}}";

	private const string GetIdsTemplate =
		"\n\tpublic static IEnumerable<string> GetIds(this LinkConfig linkDefinition)" +
		"\n\t{{\n";

	private const string GetIdsSwitch =
		"\t\tswitch (linkDefinition)" +
		"\n\t\t{{{0}\n\t\t}}\n\n";

	private const string GetIdsReturn =
		"\t\treturn new List<string>();\n\t}\n";

	private const string ContentGetIdsTemplate =
		"\n\t\t\tcase LinkConfig<{0}> link:" +
		"\n\t\t\t{{\n\t\t\t\tvar all = link.GetAll();" +
		"\n\t\t\t\treturn all.Select(l => l.{1});\n\t\t\t}}";

	private readonly TypeInfo _typeInfo;

	private readonly Dictionary<Type, List<string>> _fields = new();

	#region ICodeGenerator

	public string Create()
	{
		_fields.Clear();
		FillFields(_typeInfo, string.Empty);
		var code = GenerateCode();

		return code;
	}

	#endregion

	#region CodeGeneratorConfigLinkExtension

	public CodeGeneratorConfigLinkExtension(TypeInfo typeInfo)
	{
		_typeInfo = typeInfo;
	}
	
	private void FillFields(TypeInfo typeInfo, string path)
	{
		var fields = typeInfo.GetFields();

		foreach (var fieldInfo in fields)
		{
			if (TryGetElementType(fieldInfo, out var elementType))
			{
				AddField(elementType, $"{path}.{fieldInfo.Name}");

				continue;
			}

			if (CheckExcludedClasses(fieldInfo))
			{
				continue;
			}

			if (fieldInfo.GetType().IsClass)
			{
				var fieldTypeInfo = fieldInfo.FieldType.GetTypeInfo();
				FillFields(fieldTypeInfo, $"{path}.{fieldInfo.Name}");
			}
		}
	}

	private static bool TryGetElementType(FieldInfo fieldInfo,
		out Type elementType)
	{
		var type = fieldInfo.FieldType;

		if (!typeof(IList).IsAssignableFrom(type))
		{
			elementType = null;

			return false;
		}

		elementType = type.GetGenericArguments().First();

		if (elementType == null)
		{
			return false;
		}
		
		var fields = elementType.GetFields();

		if (CheckPrimaryKeyAttribute(fields))
		{
			return true;
		}

		elementType = null;
			
		return false;
	}

	private static bool CheckPrimaryKeyAttribute(IEnumerable<FieldInfo> fields)
	{
		var result = fields
			.Select(field => field.GetCustomAttribute<PrimaryKeyAttribute>())
			.Any(primaryKeyAttribute => primaryKeyAttribute != null);

		return result;
	}

	private void AddField(Type type, string path)
	{
		if (!_fields.ContainsKey(type))
		{
			_fields.Add(type, new List<string>());
		}

		_fields[type].Add(path);
	}

	private static bool CheckExcludedClasses(FieldInfo fieldInfo)
	{
		if (fieldInfo.FieldType.IsPrimitive)
		{
			return true;
		}
		
		if (fieldInfo.FieldType == typeof(string))
		{
			return true;
		}

		if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
		{
			return true;
		}

		if (fieldInfo.FieldType.IsArray)
		{
			return true;
		}

		if (typeof(LinkConfig).IsAssignableFrom(fieldInfo.FieldType))
		{
			return true;
		}

		return false;
	}
	
	private string GenerateCode()
	{
		var code = string.Empty;
		var definition = GenerateDefinition();
		code += definition;
		var unwrap = GenerateUnwrap();
		code += unwrap;
		var getIds = GenerateGetIds();
		code += getIds;

		return code;
	}

	private string GenerateDefinition()
	{
		return string.Format(DefinitionTemplate, _typeInfo);
	}

	private string GenerateUnwrap()
	{
		var code = string.Empty;

		foreach (var (type, namesList) in _fields)
		{
			var typeString = type.ToString().Replace('+', '.');
			var name = GetNamePrimaryKey(type);
			code += string.Format(UnwrapTemplate, typeString, name);
			var contentGetAll = GetContentGetAll(namesList);
			code += string.Format(GetAllTemplate, typeString, contentGetAll);
		}

		return code;
	}

	private string GenerateGetIds()
	{
		var code = string.Empty;
		var content = GetContentGetIds();
		code += string.Format(GetIdsTemplate);

		if (!string.IsNullOrWhiteSpace(content))
		{
			code += string.Format(GetIdsSwitch, content);
		}

		code += GetIdsReturn;

		return code;
	}

	private string GetContentGetIds()
	{
		var code = string.Empty;

		foreach (var (type, _) in _fields)
		{
			var typeString = type.ToString().Replace('+', '.');
			var name = GetNamePrimaryKey(type);
			code += string.Format(ContentGetIdsTemplate, typeString, name);
		}

		return code;
	}

	private static string GetNamePrimaryKey(Type type)
	{
		var fields = type.GetFields();

		foreach (var fieldInfo in fields)
		{
			var attribute = fieldInfo.GetCustomAttribute<PrimaryKeyAttribute>();

			if (attribute == null)
			{
				continue;
			}

			var postName = fieldInfo.FieldType != typeof(string) ? ".ToString()" : string.Empty;

			return $"{fieldInfo.Name}{postName}";
		}

		throw new InvalidOperationException();
	}

	private static string GetContentGetAll(List<string> namesList)
	{
		var content = string.Empty;

		foreach (var name in namesList)
		{
			content += string.Format(ContentGetAllTemplate, name);
		}

		return content;
	}

	public static Type GetRootType(Type type)
	{
		if (type == null || type.IsInterface)
		{
			return null;
		}

		var baseType = type.IsValueType ? typeof (ValueType) : typeof (object);

		while (baseType != type.BaseType)
		{
			type = type.BaseType;
		}

		return type;
	}

	#endregion
}

}