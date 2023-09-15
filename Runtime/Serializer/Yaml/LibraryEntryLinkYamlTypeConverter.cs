using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace EM.Configs
{

public sealed class LibraryEntryLinkYamlTypeConverter : IYamlTypeConverter
{
	private readonly ILibraryEntryCatalog _catalog;

	#region IYamlTypeConverter

	public bool Accepts(Type type)
	{
		return typeof(BaseLibraryEntryLink).IsAssignableFrom(type);
	}

	public object ReadYaml(IParser parser,
		Type type)
	{
		var deserializer  = new DeserializerBuilder().Build();
		var result = deserializer.Deserialize(parser, type);

		if (result is BaseLibraryEntryLink libraryEntryLink)
		{
			libraryEntryLink.SetCatalog(_catalog);
		}

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

	#region LibraryEntryLinkYamlTypeConverter

	public LibraryEntryLinkYamlTypeConverter(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}