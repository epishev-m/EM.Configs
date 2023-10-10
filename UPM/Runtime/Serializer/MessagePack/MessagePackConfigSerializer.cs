using System;
using System.IO;
using EM.Foundation;
using MessagePack;

namespace EM.Configs
{

public sealed class MessagePackConfigSerializer : IConfigsSerializer
{
	private readonly MessagePackSerializerOptions _options;

	#region IConfigsSerializer

	public Result Serialize<T>(Stream stream,
		T value)
		where T : class, new()
	{
		try
		{
			MessagePackSerializer.Serialize(stream, value, _options);

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
			var result = MessagePackSerializer.Deserialize<T>(stream, _options);

			return new SuccessResult<T>(result);
		}
		catch (Exception e)
		{
			return new ErrorResult<T>(e.Message);
		}
	}

	public Result Serialize<T>(string path,
		T value) where T : class, new()
	{
		throw new NotImplementedException();
	}

	public Result<T> Deserialize<T>(string path)
		where T : class, new()
	{
		throw new NotImplementedException();
	}

	#endregion

	#region MessagePackConfigSerializer

	public MessagePackConfigSerializer(MessagePackSerializerOptions options)
	{
		_options = options;
	}

	#endregion
}

}