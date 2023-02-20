namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class ConfigLink
{
	public readonly Type Type;

	public readonly string Id;

	protected ConfigLink(Type entryType,
		string id)
	{
		Requires.NotNull(entryType, nameof(entryType));
		Requires.ValidArgument(!string.IsNullOrWhiteSpace(id), "Name cannot be empty or null.");

		Type = entryType;
		Id = id;
	}
}

[Serializable]
public sealed class ConfigLink<T> : ConfigLink
	where T : class
{
	[NonSerialized]
	public T Value;

	public ConfigLink(string id)
		: base(typeof(T), id)
	{
	}
}

}