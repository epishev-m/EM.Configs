using System.Collections;
using System.Reflection;

namespace EM.Configs
{

public sealed class CollectionSizeValidatorAttribute : ValidatorAttribute
{
	private readonly int _min;

	private readonly int _max;

	#region ValidatorAttribute

	public override string ValidationType => nameof(CollectionSizeValidatorAttribute);

	public override bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value is not ICollection collection)
		{
			return true;
		}
		
		var length = collection.Count;

		if (length >= _min && length <= _max)
		{
			return true;
		}

		errorMessage = $"Collection size = {length} is out of range [{_min}:{_max}]";

		return false;
	}

	#endregion

	#region CollectionSizeValidatorAttribute

	public CollectionSizeValidatorAttribute(int min = 0, int max = int.MaxValue)
	{
		_min = min;
		_max = max;
	}

	#endregion
}

}