namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class LinkConfig
{
	public readonly Type Type;

	public abstract string Id { get; set; }

	protected LinkConfig(Type entryType)
	{
		Requires.NotNullParam(entryType, nameof(entryType));

		Type = entryType;
	}
}

[Serializable]
public abstract class LinkConfig<T> : LinkConfig
	where T : class
{
	[NonSerialized]
	public T Value;

	protected LinkConfig()
		: base(typeof(T))
	{
	}
}

}