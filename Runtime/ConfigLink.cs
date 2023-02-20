namespace EM.Configs
{

using System;
using Foundation;

[Serializable]
public abstract class ConfigLink
{
	public readonly Type Type;

	public readonly string Name;

	protected ConfigLink(Type entryType,
		string name)
	{
		Requires.NotNull(entryType, nameof(entryType));
		Requires.ValidArgument(!string.IsNullOrWhiteSpace(name), "Name cannot be empty or null.");

		Type = entryType;
		Name = name;
	}
}

[Serializable]
public sealed class ConfigLink<T> : ConfigLink
	where T : class
{
	[NonSerialized]
	public T Value;

	public ConfigLink(string name)
		: base(typeof(T), name)
	{
	}
}

}