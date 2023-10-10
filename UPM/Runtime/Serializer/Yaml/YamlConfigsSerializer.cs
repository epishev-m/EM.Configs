using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EM.Foundation;
using YamlDotNet.Serialization;

namespace EM.Configs
{

public sealed class YamlConfigsSerializer : IConfigsSerializer
{
	private readonly SerializerBuilder _serializerBuilder;

	private readonly DeserializerBuilder _deserializerBuilder;

	#region IConfigsSerializer

	public Result Serialize<T>(Stream stream,
		T value)
		where T : class, new()
	{
		try
		{
			var yaml = _serializerBuilder.Build().Serialize(value);
			using var streamWriter = new StreamWriter(stream);
			streamWriter.Write(yaml);

			return new SuccessResult();
		}
		catch (Exception exception)
		{
			return new ErrorResult(exception.Message);
		}
	}

	public Result<T> Deserialize<T>(Stream stream)
		where T : class, new()
	{
		try
		{
			using var streamReader = new StreamReader(stream);
			var value = _deserializerBuilder.Build().Deserialize<T>(streamReader);

			return new SuccessResult<T>(value);
		}
		catch (Exception exception)
		{
			return new ErrorResult<T>(exception.Message);
		}
	}

	public Result Serialize<T>(string path,
		T obj)
		where T : class, new()
	{
		var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance);

		foreach (var member in members)
		{
			if (!TryGetValue(member, obj, out var value))
			{
				continue;
			}

			var result = WriteFile(path, member.Name, value);

			if (result.Failure)
			{
				return result;
			}
		}

		return new SuccessResult();
	}

	public Result<T> Deserialize<T>(string path)
		where T : class, new()
	{
		try
		{
			var files = Directory.GetFiles(path, "*.yaml");
			var yamlStringBuilder = new StringBuilder();

			foreach (var file in files)
			{
				var str = File.ReadAllText(file);
				yamlStringBuilder.AppendLine(str);
			}

			var yaml = yamlStringBuilder.ToString();
			var result = _deserializerBuilder.Build().Deserialize<T>(yaml);

			return new SuccessResult<T>(result);
		}
		catch (Exception exception)
		{
			return new ErrorResult<T>(exception.Message);
		}
	}

	#endregion

	#region YamlConfigsSerializer

	public YamlConfigsSerializer(SerializerBuilder serializerBuilder,
		DeserializerBuilder deserializerBuilder)
	{
		_serializerBuilder = serializerBuilder;
		_deserializerBuilder = deserializerBuilder;
	}

	private static bool TryGetValue(MemberInfo memberInfo,
		object obj,
		out object value)
	{
		value = null;

		if (memberInfo is FieldInfo fieldInfo)
		{
			value = fieldInfo.GetValue(obj);
		}
		else if (memberInfo is PropertyInfo propertyInfo)
		{
			if (propertyInfo.CanRead)
			{
				return false;
			}

			value = propertyInfo.GetValue(obj);
		}

		return true;
	}

	private Result WriteFile(string path,
		string name,
		object value)
	{
		var dictionary = new Dictionary<string, object>
		{
			{name, value}
		};

		try
		{
			var yaml = _serializerBuilder.Build().Serialize(dictionary);
			var filePath = Path.Combine(path, $"{name}.yaml");
			using var streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
			streamWriter.Write(yaml);

			return new SuccessResult();
		}
		catch (IOException exception)
		{
			return new ErrorResult(exception.Message);
		}
	}
	
	#endregion
}

}