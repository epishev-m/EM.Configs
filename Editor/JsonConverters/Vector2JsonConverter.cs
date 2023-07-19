using System;
using Newtonsoft.Json;
using UnityEngine;

namespace EM.Configs.Editor
{
public sealed class Vector2JsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Vector2);
	}

	public override object ReadJson(JsonReader reader,
		Type objectType,
		object existingValue,
		JsonSerializer serializer)
	{
		var obj = serializer.Deserialize(reader);

		if (obj == null)
		{
			return null;
		}

		var vector2 = JsonConvert.DeserializeObject<Vector2>(obj.ToString());

		return vector2;
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();

			return;
		}

		var v = (Vector2)value;

		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(v.x);
		writer.WritePropertyName("y");
		writer.WriteValue(v.y);
		writer.WriteEndObject();
	}
}

}