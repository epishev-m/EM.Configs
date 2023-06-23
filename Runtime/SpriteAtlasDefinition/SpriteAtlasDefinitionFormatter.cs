namespace EM.Configs
{

using System;
using MessagePack;
using MessagePack.Formatters;

public class SpriteAtlasDefinitionFormatter<TSpriteAtlas> : IMessagePackFormatter<TSpriteAtlas>
	where TSpriteAtlas : ISpriteAtlas
{
	public void Serialize(ref MessagePackWriter writer,
		TSpriteAtlas value,
		MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
		}
		else
		{
			writer.WriteArrayHeader(2);
			var resolver = options.Resolver;
			resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Atlas, options);
			resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Sprite, options);
		}
	}

	public TSpriteAtlas Deserialize(ref MessagePackReader reader,
		MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return default;
		}

		var count = reader.ReadArrayHeader();
		
		if (count != 2)
		{
			throw new MessagePackSerializationException("Invalid SpriteAtlasDefinition count");
		}

		var resolver = options.Resolver;
		options.Security.DepthStep(ref reader);
		
		try
		{
			var atlas = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
			var sprite = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);

			var spriteAtlas = Activator.CreateInstance<TSpriteAtlas>();
			spriteAtlas.Atlas = atlas;
			spriteAtlas.Sprite = sprite;

			return spriteAtlas;
		}
		finally
		{
			reader.Depth--;
		}
	}
}

}