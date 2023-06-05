namespace EM.Configs
{

using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class SpriteAtlasAttribute : Attribute
{
	public readonly string Address;

	#region ArrayLengthAttribute

	public SpriteAtlasAttribute(string address)
	{
		Address = address;
	}

	#endregion
}

}