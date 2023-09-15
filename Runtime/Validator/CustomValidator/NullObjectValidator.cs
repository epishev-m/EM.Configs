using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EM.Configs
{

public sealed class NullObjectValidator : ICustomValidator
{
	public string ValidationType => nameof(NullObjectValidator);

	public bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value != null)
		{
			return TryValidateCollection(value, out errorMessage);
		}

		var optionalFieldAttribute = fieldInfo.GetCustomAttribute<OptionalFieldAttribute>();

		if (optionalFieldAttribute == null)
		{
			errorMessage = "Field must not be NULL.";
		}

		return string.IsNullOrWhiteSpace(errorMessage);
	}

	private static bool TryValidateCollection(object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value is not ICollection collection)
		{
			return true;
		}

		var nullIndexesList = new List<int>();
		var index = 0;

		foreach (var collectionObject in collection)
		{
			if (collectionObject != null)
			{
				continue;
			}

			nullIndexesList.Add(index);
			index++;
		}

		if (nullIndexesList.Count <= 0)
		{
			return true;
		}

		var stringBuilder = new StringBuilder("The collection cannot contain null objects. Check indexes: ");

		foreach (var nullIndex in nullIndexesList)
		{
			if (nullIndex > 0)
			{
				stringBuilder.Append(" ,");
			}

			stringBuilder.Append(nullIndex);
		}

		errorMessage = stringBuilder.ToString();

		return false;
	}
}

}