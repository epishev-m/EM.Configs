using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace EM.Configs
{

public sealed class LibraryEntryYamlTypeConverter : IYamlTypeConverter
{
	private readonly ILibraryEntryCatalog _catalog;

	#region IYamlTypeConverter

	public bool Accepts(Type type)
	{
		return typeof(ILibraryEntry).IsAssignableFrom(type);
	}

	public object ReadYaml(IParser parser,
		Type type)
	{
		var deserializer  = new DeserializerBuilder().Build();
		var result = (ILibraryEntry) deserializer.Deserialize(parser, type);
		_catalog.Register(result);

		return result;
	}

	public void WriteYaml(IEmitter emitter,
		object value,
		Type type)
	{
		var serializer = new SerializerBuilder().Build();
		serializer.Serialize(emitter, value, type);
	}

	#endregion

	#region LibraryEntryYamlTypeConverter

	public LibraryEntryYamlTypeConverter(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}