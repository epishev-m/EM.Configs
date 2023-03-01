namespace EM.Configs
{

using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CollectionSizeAttribute : Attribute
{
	public readonly int Min;

	public readonly int Max;

	#region ArrayLengthAttribute

	public CollectionSizeAttribute(int min = 0, int max = int.MaxValue)
	{
		Min = min;
		Max = max;
	}

	#endregion
}

}