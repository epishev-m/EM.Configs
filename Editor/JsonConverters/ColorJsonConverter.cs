using DG.DemiEditor;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace EM.Configs.Editor
{
	
public class ColorJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(Color).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		var color = (Color) value;
		writer.WriteValue($"#{color.ToHex(true)}");
	}

	public override object ReadJson(JsonReader reader,
		Type objectType,
		object existingValue,
		JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}

		var hex = (string) reader.Value;
		ColorUtility.TryParseHtmlString(hex, out var color);

		return color;
	}
}

}