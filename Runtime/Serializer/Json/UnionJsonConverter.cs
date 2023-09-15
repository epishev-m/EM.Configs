using System;
using System.Linq;
#if EM_USE_MESSAGE_PACK
using MessagePack;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EM.Configs
{

public sealed class UnionJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return Attribute.GetCustomAttributes(objectType).Any(v => v is UnionAttribute);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override object ReadJson(JsonReader reader,
		Type objectType,
		object existingValue,
		JsonSerializer serializer)
	{
		var jObject = JObject.Load(reader);
		var attrs = Attribute.GetCustomAttributes(objectType);

		foreach (var attribute in attrs)
		{
			if (attribute is not UnionAttribute unionAttribute)
			{
				continue;
			}

			var fields = unionAttribute.SubType.GetFields();
			var found = true;
			var fieldsCounter = 0;

			foreach (var obj in jObject)
			{
				if (fields.Any(z => z.Name == obj.Key))
				{
					fieldsCounter++;
					continue;
				}

				found = false;

				break;
			}

			if (fieldsCounter != fields.Length)
			{
				found = false;
			}

			if (!found)
			{
				continue;
			}

			var target = Activator.CreateInstance(unionAttribute.SubType);
			serializer.Populate(jObject.CreateReader(), target);

			return target;
		}

		throw new InvalidOperationException();
	}
}

}