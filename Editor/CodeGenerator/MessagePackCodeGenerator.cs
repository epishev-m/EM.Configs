namespace EM.Configs.Editor
{

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public sealed class MessagePackCodeGenerator
{
	private readonly string _inputPath;

	private readonly string _outputPath;

	public MessagePackCodeGenerator(string inputPath,
		string outputPath)
	{
		_inputPath = inputPath;
		_outputPath = outputPath;
	}

	public void Execute(Action collback)
	{
		var psi = new ProcessStartInfo
		{
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			FileName = "mpc",
			Arguments = $"-i {_inputPath} -o {_outputPath}/MessagePackGenerated.cs -n EM.Configs",
			WorkingDirectory = Application.dataPath
		};

		Process process;

		try
		{
			process = Process.Start(psi);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError(e.Message);
			return;
		}

		if (process == null)
		{
			collback?.Invoke();

			return;
		}

		process.EnableRaisingEvents = true;
		process.Exited += (_, _) =>
		{
			process.Dispose();
			process = null;
			collback?.Invoke();
		};
	}
}

}