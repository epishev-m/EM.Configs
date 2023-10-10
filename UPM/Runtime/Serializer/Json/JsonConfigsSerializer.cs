using System;
using System.IO;
using System.Text;
using EM.Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EM.Configs
{

public sealed class JsonConfigsSerializer : IConfigsSerializer
{
	private readonly JsonSerializerSettings _settings;

	#region IConfigsSerializer

	public Result Serialize<T>(Stream stream,
		T value)
		where T : class, new()
	{
		try
		{
			var json = JsonConvert.SerializeObject(value, _settings);
			using var streamWriter = new StreamWriter(stream);
			streamWriter.Write(json);

			return new SuccessResult();
		}
		catch (Exception e)
		{
			return new ErrorResult(e.Message);
		}
	}

	public Result<T> Deserialize<T>(Stream stream)
		where T : class, new()
	{
		try
		{
			using var streamReader = new StreamReader(stream);
			var json = streamReader.ReadToEnd();
			var value = JsonConvert.DeserializeObject<T>(json, _settings);

			return new SuccessResult<T>(value);
		}
		catch (Exception exception)
		{
			return new ErrorResult<T>(exception.Message);
		}
	}

	public Result Serialize<T>(string path,
		T value) where T : class, new()
	{
		Requires.NotNullParam(value, nameof(value));

		try
		{
			var json = JsonConvert.SerializeObject(value, _settings);
			var jObject = JObject.Parse(json);

			foreach (var (key, jToken) in jObject)
			{
				if (jToken == null)
				{
					continue;
				}

				var jObjectField = new JObject
				{
					{key, jToken}
				};

				var resultJson = jObjectField.ToString();
				var filePath = $"{path}/{key}.json";
				using var outputFile = new StreamWriter(filePath, false, Encoding.UTF8);
				outputFile.Write(resultJson);
			}

			return new SuccessResult();
		}
		catch (IOException exception)
		{
			return new ErrorResult(exception.Message);
		}
	}

	public Result<T> Deserialize<T>(string path)
		where T : class, new()
	{
		try
		{
			var config = new T();
			var dir = new DirectoryInfo(path);
			var files = dir.GetFiles("*.json");

			foreach (var fileInfo in files)
			{
				var json = File.ReadAllText(fileInfo.FullName);
				JsonConvert.PopulateObject(json, config, _settings);
			}
			
			return new SuccessResult<T>(config);
		}
		catch (Exception exception)
		{
			return new ErrorResult<T>(exception.Message);
		}
	}

	#endregion

	#region JsonConfigsSerializer

	public JsonConfigsSerializer(JsonSerializerSettings settings)
	{
		_settings = settings;
	}

	#endregion
}

}