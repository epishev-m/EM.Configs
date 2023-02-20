﻿namespace EM.Configs
{

using System;
using Newtonsoft.Json;

public class ConfigLinkJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(ConfigLink).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		writer.WriteValue(((ConfigLink) value).Id);
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

		var name = (string) reader.Value;
		var resultObj = Activator.CreateInstance(objectType, name);

		return resultObj;
	}
}

}