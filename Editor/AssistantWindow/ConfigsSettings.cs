namespace EM.Configs.Editor
{

using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ConfigsSettings), menuName = "Game/Configs Settings")]
public sealed class ConfigsSettings : ScriptableObject
{
	public string InputPath;

	public string OutputPath;

	public string CodeGenerationPath;

	#region ConfigsSettings

	public string FullOutputPath
	{
		get
		{
			var fullPath = Path.GetFullPath(OutputPath, Application.dataPath);
			
			return fullPath;
		}
	}

	public string FullCodeGenerationPath
	{
		get
		{
			var fullPath = Path.GetFullPath(CodeGenerationPath, Application.dataPath);
			
			return fullPath;
		}
	}

	#endregion
}

}