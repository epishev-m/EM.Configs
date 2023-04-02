namespace EM.Configs.Editor
{

using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ConfigsSettings), menuName = "Game/Configs Settings")]
public sealed class ConfigsSettings : ScriptableObject
{
	public string InputPath;

	public string OutputPath;

	public string CodeGenerationInputPath;

	public string CodeGenerationOutputPath;

	#region ConfigsSettings

	public string FullOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(OutputPath, Application.dataPath);
			
			return fullPath;
		}
	}

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