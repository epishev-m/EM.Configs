namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class ConfigLink
{
	public readonly Type Type;

	public readonly string Id;

	#region ConfigLink

	protected ConfigLink(Type entryType,
		string id)
	{
		Requires.NotNullParam(entryType, nameof(entryType));
		Requires.ValidArgument(!string.IsNullOrWhiteSpace(id), "Name cannot be empty or null.");

		Type = entryType;
		Id = id;
	}

	#endregion
}

[Serializable]
public sealed class ConfigLink<T> : ConfigLink
	where T : class
{
	[NonSerialized] public T Value;

	#region ConfigLink

	public ConfigLink(string id)
		: base(typeof(T), id)
	{
	}

	#endregion
}

}