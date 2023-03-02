namespace EM.Configs
{

using System;

public interface IRangeAttribute<T>
	where T : struct, IComparable
{
	T Min
	{
		get;
	}

	T Max
	{
		get;
	}

	public bool Check(T value)
	{
		var min = value.CompareTo(Min);
		var max = value.CompareTo(Max);
		return min >= 0 && max <= 0;
	}
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeIntAttribute : Attribute,
	IRangeAttribute<int>
{
	#region IRangeAttribute

	public int Min { get; }
	public int Max { get; }
	
	#endregion
	
	#region RangeIntAttribute

	public RangeIntAttribute(int min = int.MinValue, int max = int.MaxValue)
	{
		Min = min;
		Max = max;
	}

	#endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeLongAttribute : Attribute,
	IRangeAttribute<long>
{
	#region IRangeAttribute

	public long Min { get; }
	public long Max { get; }
	
	#endregion
	
	#region RangeIntAttribute

	public RangeLongAttribute(long min = long.MinValue, long max = long.MaxValue)
	{
		Min = min;
		Max = max;
	}

	#endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeFloatAttribute : Attribute,
	IRangeAttribute<float>
{
	#region IRangeAttribute

	public float Min { get; }
	public float Max { get; }
	
	#endregion
	
	#region RangeIntAttribute

	public RangeFloatAttribute(float min = float.MinValue, float max = float.MaxValue)
	{
		Min = min;
		Max = max;
	}

	#endregion
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeDoubleAttribute : Attribute,
	IRangeAttribute<double>
{
	#region IRangeAttribute

	public double Min { get; }
	public double Max { get; }
	
	#endregion
	
	#region RangeIntAttribute

	public RangeDoubleAttribute(double min = double.MinValue, double max = double.MaxValue)
	{
		Min = min;
		Max = max;
	}

	#endregion
}

}