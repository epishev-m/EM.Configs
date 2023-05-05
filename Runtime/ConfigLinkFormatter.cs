namespace EM.Configs
{

using MessagePack;
using MessagePack.Formatters;

public sealed class ConfigLinkFormatter<T> : IMessagePackFormatter<ConfigLink<T>>
	where T : class
{
	#region IMessagePackFormatter

	public void Serialize(ref MessagePackWriter writer,
		ConfigLink<T> value,
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

	public ConfigLink<T> Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return default;
		}

		var resolver = options.Resolver;
		var stringFormatter = resolver.GetFormatterWithVerify<string>();
		var id =  stringFormatter.Deserialize(ref reader, options);

		var link = new ConfigLink<T>
		{
			Id = id
		};

		return link;
	}

	#endregion
}

}