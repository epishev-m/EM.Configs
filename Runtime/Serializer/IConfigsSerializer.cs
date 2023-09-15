using System.IO;
using EM.Foundation;

namespace EM.Configs
{

public interface IConfigsSerializer
{
	Result Serialize<T>(Stream stream,
		T value)
		where T : class, new();

	Result<T> Deserialize<T>(Stream stream)
		where T : class, new();

	Result Serialize<T>(string path,
		T value)
		where T : class, new();

	Result<T> Deserialize<T>(string path)
		where T : class, new();
}

}