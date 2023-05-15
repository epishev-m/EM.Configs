namespace EM.Configs
{

using MessagePack;
using MessagePack.Formatters;

public sealed class DefinitionLinkFormatter<T> : IMessagePackFormatter<DefinitionLink<T>>
	where T : class
{
	public void Serialize(ref MessagePackWriter writer,
		DefinitionLink<T> value,
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

	public DefinitionLink<T> Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return default;
		}

		var resolver = options.Resolver;
		var stringFormatter = resolver.GetFormatterWithVerify<string>();
		var id =  stringFormatter.Deserialize(ref reader, options);

		var link = new DefinitionLink<T>
		{
			Id = id
		};

		return link;
	}
}

}