namespace EM.Configs.Editor
{

using System;
using Newtonsoft.Json;

public class ConfigLinkJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(LinkConfig).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		writer.WriteValue(((LinkConfig) value).Id);
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

		var key = (string) reader.Value;
		var resultObj = (LinkConfig) Activator.CreateInstance(objectType);
		resultObj.Id = key;

		return resultObj;
	}
}

}