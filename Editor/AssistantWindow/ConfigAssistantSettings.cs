namespace EM.Configs.Editor
{

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class ConfigAssistantSettings
{
	public string InputPath;

	public string OutputPath;

	public string MessagePackInputPath;

	public string MessagePackOutputPath;

	public string LinkConfigOutputPath;

	#region ConfigAssistantSettings

	[JsonIgnore]
	public string FullOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(OutputPath, Application.dataPath);

			return fullPath;
		}
	}

	[JsonIgnore]
	public string MessagePackFullOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(MessagePackOutputPath, Application.dataPath);

			return fullPath;
		}
	}

	[JsonIgnore]
	public string LinkConfigFullOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(LinkConfigOutputPath, Application.dataPath);

			return fullPath;
		}
	}

	#endregion
}

}