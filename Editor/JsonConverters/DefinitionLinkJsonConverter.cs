namespace EM.Configs.Editor
{

using System;
using Newtonsoft.Json;

public class DefinitionLinkJsonConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(DefinitionLink).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		writer.WriteValue(((DefinitionLink) value).Id);
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
		var resultObj = (DefinitionLink) Activator.CreateInstance(objectType);
		resultObj.Id = key;

		return resultObj;
	}
}

}