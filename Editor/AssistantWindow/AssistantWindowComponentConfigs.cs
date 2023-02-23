namespace EM.Configs.Editor
{

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using EM.Foundation.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public sealed class AssistantWindowComponentConfigs<T> : ScriptableObjectAssistantWindowComponent<ConfigsSettings>
	where T : class, new()
{
	private readonly IEnumerable<IConfigsValidator> _validators;

	#region ScriptableObjectAssistantWindowComponent

	public override string Name => "Configs";

	protected override string GetCreatePath()
	{
		var path = EditorUtility.SaveFilePanelInProject("Create Configs Settings",
			"ConfigsSettings.asset", "asset", "");

		return path;
	}

	protected override string GetSelectPath()
	{
		var path = EditorUtility.OpenFilePanel("Select Configs Settings", "Assets", "asset");

		return path;
	}

	protected override void OnGUIConfig()
	{
		OnGuiInputPath();
		OnGuiOutputPath();
		OnGuiCodeGenerationPath();
		OnGuiButtons();
	}

	#endregion

	#region AssistantWindowComponentConfigs

	public AssistantWindowComponentConfigs(IEnumerable<IConfigsValidator> validators)
	{
		_validators = validators;
	}

	private void OnGuiInputPath()
	{
		EditorGUILayout.LabelField("Input path:");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.InputPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("Input path", "Assets", string.Empty);

				if (!string.IsNullOrWhiteSpace(fullPath))
				{
					Settings.InputPath = Path.GetRelativePath(Application.dataPath, fullPath);
				}
			}
		}
	}

	private void OnGuiOutputPath()
	{
		EditorGUILayout.LabelField("Output path:");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.OutputPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("Output path", "Assets", "");

				if (!string.IsNullOrWhiteSpace(fullPath))
				{
					Settings.OutputPath = Path.GetRelativePath(Application.dataPath, fullPath) + @"\Configs.bytes";
				}
			}
		}
	}

	private void OnGuiCodeGenerationPath()
	{
		EditorGUILayout.LabelField("Code Generation path:");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.CodeGenerationPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("Code Generation Path", "Assets", string.Empty);

				if (!string.IsNullOrWhiteSpace(fullPath))
				{
					Settings.CodeGenerationPath = Path.GetRelativePath(Application.dataPath, fullPath);
				}
			}
		}
	}

	private void OnGuiButtons()
	{
		EditorGUILayout.Space();

		using (new EditorVerticalGroup())
		{
			if (GUILayout.Button("Code Generate"))
			{
				CodeGenerate();
			}

			if (GUILayout.Button("Configure"))
			{
				var config = CreateConfig();
				ValidateConfig(config);
				SaveConfig(config);
				SetAddressableFlag();
			}
		}
	}

	private T CreateConfig()
	{
		var config = new T();
		var fullInputPath = Path.GetFullPath(Settings.InputPath, Application.dataPath);
		var dir = new DirectoryInfo(fullInputPath);
		var files = dir.GetFiles("*.json");

		JsonSerializerSettings jsonSettings = new()
		{
			Converters =
			{
				new ConfigLinkJsonConverter(),
				new UnionConverter()
			}
		};

		foreach (var fileInfo in files)
		{
			var json = File.ReadAllText(fileInfo.FullName);
			JsonConvert.PopulateObject(json, config, jsonSettings);
		}

		return config;
	}

	private void ValidateConfig(T config)
	{
		var stringBuilder = new StringBuilder();
		foreach (var validator in _validators)
		{
			if (!validator.Validate(config))
			{
				stringBuilder.AppendLine(validator.ErrorMassage);
			}
		}

		var errors = stringBuilder.ToString();

		if (string.IsNullOrWhiteSpace(errors))
		{
			Debug.Log("Successful config validation!");
		}
		else
		{
			Debug.LogError("Config validation failed!\n\n" + errors);
		}
	}

	private void SaveConfig(T config)
	{
		var formatter = new BinaryFormatter();
		using var fs = new FileStream(Settings.FullOutputPath, FileMode.OpenOrCreate);
		formatter.Serialize(fs, config);
		AssetDatabase.Refresh();
	}

	private void SetAddressableFlag()
	{
		var guiID = AssetDatabase.AssetPathToGUID(@"Assets\" + Settings.OutputPath);
		var settings = AddressableAssetSettingsDefaultObject.Settings;
		var assetEntry = settings.CreateOrMoveEntry(guiID, settings.DefaultGroup);
		var name = Path.GetFileNameWithoutExtension(Settings.FullOutputPath);
		assetEntry.address = name;
	}

	private void CodeGenerate()
	{
		var codeGenerator = new ConfigsCodeGenerator(typeof(T).GetTypeInfo(), Settings.FullCodeGenerationPath);
		codeGenerator.Execute();
	}

	#endregion
}

}