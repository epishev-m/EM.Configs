using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EM.Configs
{

public sealed class LibraryEntryJsonConverter : JsonConverter
{
	private readonly ILibraryEntryCatalog _catalog;

	#region JsonConverter

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(ILibraryEntry).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
	}

	public override object ReadJson(JsonReader reader,
		Type objectType,
		object existingValue,
		JsonSerializer serializer)
	{
		var jo = JObject.Load(reader);
		var value = (ILibraryEntry) jo.ToObject(objectType);
		_catalog.Register(value);

		return value;
	}

	#endregion

	#region LibraryEntryJsonConverter

	public LibraryEntryJsonConverter(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}