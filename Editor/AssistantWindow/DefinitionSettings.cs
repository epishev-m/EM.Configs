namespace EM.Configs.Editor
{

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public sealed class DefinitionSettings
{
	public string InputPath;

	public string OutputPath;

	public string CodeGenerationInputPath;

	public string CodeGenerationOutputPath;

	#region ConfigsSettings

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
	public string FullCodeGenerationOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(CodeGenerationOutputPath, Application.dataPath);

			return fullPath;
		}
	}

	#endregion
}

}