namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class LinkDefinition
{
	public readonly Type Type;

	public string Id;

	#region LinkDefinition

	protected LinkDefinition(Type entryType)
	{
		Requires.NotNullParam(entryType, nameof(entryType));

		Type = entryType;
	}

	#endregion
}

[Serializable]
public class LinkDefinition<T> : LinkDefinition
	where T : class
{
	[NonSerialized] public T Value;

	#region LinkDefinition

	public LinkDefinition()
		: base(typeof(T))
	{
	}

	#endregion
}

}