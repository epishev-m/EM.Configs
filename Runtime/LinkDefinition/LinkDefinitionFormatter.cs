namespace EM.Configs
{

using MessagePack;
using MessagePack.Formatters;

public sealed class LinkDefinitionFormatter<T> : IMessagePackFormatter<LinkDefinition<T>>
	where T : class
{
	public void Serialize(ref MessagePackWriter writer,
		LinkDefinition<T> value,
		MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
		}
		else
		{
			writer.Write(value.Id);
		}
	}

	public LinkDefinition<T> Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return default;
		}

		var resolver = options.Resolver;
		var stringFormatter = resolver.GetFormatterWithVerify<string>();
		var id =  stringFormatter.Deserialize(ref reader, options);

		var link = new LinkDefinition<T>
		{
			Id = id
		};

		return link;
	}
}

}