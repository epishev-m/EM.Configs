namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class DefinitionLink
{
	public readonly Type Type;

	public string Id;

	#region ConfigLink

	protected DefinitionLink(Type entryType)
	{
		Requires.NotNullParam(entryType, nameof(entryType));

		Type = entryType;
	}

	#endregion
}

[Serializable]
public sealed class DefinitionLink<T> : DefinitionLink
	where T : class
{
	[NonSerialized] public T Value;

	#region ConfigLink

	public DefinitionLink()
		: base(typeof(T))
	{
	}

	#endregion
}

}