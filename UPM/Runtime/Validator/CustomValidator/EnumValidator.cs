using System;
using System.Collections;
using System.Reflection;

namespace EM.Configs
{

public sealed class EnumValidator : ICustomValidator
{
	public string ValidationType => nameof(EnumValidator);

	public bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;
		var type = fieldInfo.FieldType;

		if (!type.IsEnum)
		{
			return true;
		}

		var values = Enum.GetValues(type) as IList;

		if (values.Count <= 0)
		{
			return true;
		}

		var rawValue = Convert.ToInt64(value);
		var rawItemNone = Convert.ToInt64(values[0]);

		if (rawValue != rawItemNone)
		{
			return true;
		}

		var optionalField = fieldInfo.GetCustomAttribute<OptionalFieldAttribute>();

		if (optionalField != null)
		{
			return true;
		}

		errorMessage = "The enum value must not be \"0\" (None)";

		return string.IsNullOrWhiteSpace(errorMessage);
	}
}

}