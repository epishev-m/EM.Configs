#if !EM_USE_MESSAGE_PACK
using System;

namespace EM.Configs
{

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UnionAttribute : Attribute
{
	private readonly int _key;

	public readonly Type SubType;

	public UnionAttribute(int key,
		Type subType)
	{
		_key = key;
		SubType = subType ?? throw new ArgumentNullException(nameof(subType));
	}

	public UnionAttribute(int key,
		string subType)
	{
		_key = key;
		SubType = Type.GetType(subType, throwOnError: true);
	}
}

}
#endif