using System.Reflection;

namespace EM.Configs
{

public sealed class RangeIntValidatorAttribute : ValidatorAttribute
{
	private readonly int _min;

	private readonly int _max;

	#region ValidatorAttribute

	public override string ValidationType => nameof(RangeIntValidatorAttribute);

	public override bool TryValidate(FieldInfo fieldInfo,
		object instance,
		out string errorMessage)
	{
		errorMessage = string.Empty;
		var fieldType = fieldInfo.FieldType;

		if (!fieldType.IsPrimitive)
		{
			return true;
		}

		if (fieldType != typeof(int))
		{
			return true;
		}

		var value = (int) instance;
		var min = value.CompareTo(_min);
		var max = value.CompareTo(_max);

		if (min >= 0 && max <= 0)
		{
			return true;
		}

		errorMessage = $"Value = {value} is out of range [{_min}:{_max}].";

		return false;
	}

	#endregion

	#region RangeIntValidatorAttribute

	public RangeIntValidatorAttribute(int min = int.MinValue,
		int max = int.MaxValue)
	{
		_min = min;
		_max = max;
	}

	#endregion
}

public sealed class RangeLongValidatorAttribute : ValidatorAttribute
{
	private readonly long _min;

	private readonly long _max;

	#region ValidatorAttribute

	public override string ValidationType => nameof(RangeLongValidatorAttribute);

	public override bool TryValidate(FieldInfo fieldInfo,
		object instance,
		out string errorMessage)
	{
		errorMessage = string.Empty;
		var fieldType = fieldInfo.FieldType;

		if (!fieldType.IsPrimitive)
		{
			return true;
		}

		if (fieldType != typeof(long))
		{
			return true;
		}

		var value = (long) instance;
		var min = value.CompareTo(_min);
		var max = value.CompareTo(_max);

		if (min >= 0 && max <= 0)
		{
			return true;
		}

		errorMessage = $"Value = {value} is out of range [{_min}:{_max}].";

		return false;
	}

	#endregion

	#region RangeLongValidatorAttribute

	public RangeLongValidatorAttribute(long min = long.MinValue,
		long max = long.MaxValue)
	{
		_min = min;
		_max = max;
	}

	#endregion
}

public sealed class RangeFloatValidatorAttribute : ValidatorAttribute
{
	private readonly float _min;

	private readonly float _max;

	#region ValidatorAttribute

	public override string ValidationType => nameof(RangeFloatValidatorAttribute);

	public override bool TryValidate(FieldInfo fieldInfo,
		object instance,
		out string errorMessage)
	{
		errorMessage = string.Empty;
		var fieldType = fieldInfo.FieldType;

		if (!fieldType.IsPrimitive)
		{
			return true;
		}

		if (fieldType != typeof(float))
		{
			return true;
		}

		var value = (float) instance;
		var min = value.CompareTo(_min);
		var max = value.CompareTo(_max);

		if (min >= 0 && max <= 0)
		{
			return true;
		}

		errorMessage = $"Value = {value} is out of range [{_min}:{_max}].";

		return false;
	}

	#endregion

	#region RangeFloatValidatorAttribute

	public RangeFloatValidatorAttribute(float min = float.MinValue,
		float max = float.MaxValue)
	{
		_min = min;
		_max = max;
	}

	#endregion
}

public sealed class RangeDoubleValidatorAttribute : ValidatorAttribute
{
	private readonly double _min;

	private readonly double _max;

	#region ValidatorAttribute

	public override string ValidationType => nameof(RangeDoubleValidatorAttribute);

	public override bool TryValidate(FieldInfo fieldInfo,
		object instance,
		out string errorMessage)
	{
		errorMessage = string.Empty;
		var fieldType = fieldInfo.FieldType;

		if (!fieldType.IsPrimitive)
		{
			return true;
		}

		if (fieldType != typeof(double))
		{
			return true;
		}

		var value = (double) instance;
		var min = value.CompareTo(_min);
		var max = value.CompareTo(_max);

		if (min >= 0 && max <= 0)
		{
			return true;
		}

		errorMessage = $"Value = {value} is out of range [{_min}:{_max}].";

		return false;
	}

	#endregion

	#region RangeDoubleValidatorAttribute

	public RangeDoubleValidatorAttribute(double min = double.MinValue,
		double max = double.MaxValue)
	{
		_min = min;
		_max = max;
	}

	#endregion
}

}