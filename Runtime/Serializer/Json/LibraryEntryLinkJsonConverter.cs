using System;
using Newtonsoft.Json;

namespace EM.Configs
{

public class LibraryEntryLinkJsonConverter : JsonConverter
{
	private readonly ILibraryEntryCatalog _catalog;

	#region JsonConverter

	public override bool CanConvert(Type objectType)
	{
		return typeof(BaseLibraryEntryLink).IsAssignableFrom(objectType);
	}

	public override void WriteJson(JsonWriter writer,
		object value,
		JsonSerializer serializer)
	{
		writer.WriteValue(((BaseLibraryEntryLink) value).Id);
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
		var resultObj = (BaseLibraryEntryLink) Activator.CreateInstance(objectType);
		resultObj.Id = key;
		resultObj.SetCatalog(_catalog);

		return resultObj;
	}

	#endregion

	#region LibraryEntryLinkJsonConverter

	public LibraryEntryLinkJsonConverter(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}