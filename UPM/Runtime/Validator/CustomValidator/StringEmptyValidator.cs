using System.Reflection;

namespace EM.Configs
{

public sealed class StringEmptyValidator : ICustomValidator
{
	public string ValidationType => nameof(StringEmptyValidator);

	public bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value == null)
		{
			return true;
		}

		if (value is not string stringValue)
		{
			return true;
		}

		if (!string.IsNullOrWhiteSpace(stringValue))
		{
			return true;
		}

		var optionalFieldAttribute = fieldInfo.GetCustomAttribute<OptionalFieldAttribute>();

		if (optionalFieldAttribute == null)
		{
			errorMessage = "String cannot be empty.";
		}

		return string.IsNullOrWhiteSpace(errorMessage);
	}
}

}