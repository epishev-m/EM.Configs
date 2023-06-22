namespace EM.Configs
{

using System;
using MessagePack;
using MessagePack.Formatters;

public sealed class LinkDefinitionFormatter<TLink> : IMessagePackFormatter<TLink>
	where TLink : LinkDefinition
{
	public void Serialize(ref MessagePackWriter writer,
		TLink value,
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

	public TLink Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return default;
		}

		var resolver = options.Resolver;
		var stringFormatter = resolver.GetFormatterWithVerify<string>();
		var id =  stringFormatter.Deserialize(ref reader, options);

		var link = Activator.CreateInstance<TLink>();
		link.Id = id;

		return link;
	}
}

}