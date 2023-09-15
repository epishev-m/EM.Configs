using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace EM.Configs
{

public sealed class LibraryEntryLinkFormatter<T> : IMessagePackFormatter<T>,
	ILibraryEntryCatalogProvider
	where T : BaseLibraryEntryLink
{
	private readonly IFormatterResolver _resolver;

	#region ILibraryEntryCatalogProvider

	public ILibraryEntryCatalog Catalog
	{
		get;
		set;
	}

	#endregion

	#region IMessagePackFormatter

	public void Serialize(ref MessagePackWriter writer,
		T value,
		MessagePackSerializerOptions options)
	{
		var formatter = _resolver.GetFormatter<T>();

		if (formatter != null)
		{
			formatter.Serialize(ref writer, value, options);
		}
		else
		{
			writer.WriteNil();
		}
	}

	public T Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		var formatter = _resolver.GetFormatter<T>();

		if (formatter == null)
		{
			return default;
		}

		var result = formatter.Deserialize(ref reader, options);
		result.SetCatalog(Catalog);

		return result;
	}

	#endregion

	#region LibraryEntryFormatter

	public LibraryEntryLinkFormatter(IFormatterResolver resolver)
	{
		_resolver = resolver;
	}

	#endregion
}

}