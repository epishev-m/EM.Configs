namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class ConfigLink
{
	public readonly Type Type;

	public string Id;

	#region ConfigLink

	protected ConfigLink(Type entryType)
	{
		Requires.NotNullParam(entryType, nameof(entryType));

		Type = entryType;
	}

	#endregion
}

[Serializable]
public sealed class ConfigLink<T> : ConfigLink
	where T : class
{
	[NonSerialized] public T Value;

	#region ConfigLink

	public ConfigLink()
		: base(typeof(T))
	{
	}

	#endregion
}

}