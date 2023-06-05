﻿namespace EM.Configs.Editor
{

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assistant.Editor;
using MessagePack;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public sealed class DefinitionGeneralAssistantComponent<T> : ProjectSettingsAssistantComponent<DefinitionSettings>
	where T : class, new()
{
	private readonly IEnumerable<IConfigsValidator> _validators;

	private bool _invokingCodeGen;

	private bool _assetDbRefresh;

	#region ProjectSettingsAssistantComponent

	public override string Name => "Definition General";

	protected override string DirectoryPath => "ProjectSettings/";

	protected override void OnGUIConfig()
	{
		if (_assetDbRefresh)
		{
			_assetDbRefresh = false;
			AssetDatabase.Refresh();
		}

		using (new EditorDisabledGroup(_invokingCodeGen))
		{
			OnGuiInputPath();
			OnGuiOutputPath();
			OnGuiCodeGenerationInputPath();
			OnGuiCodeGenerationOutputPath();
			OnGuiButtons();
		}
	}

	#endregion

	#region AssistantWindowComponentConfigs

	public DefinitionGeneralAssistantComponent(IEnumerable<IConfigsValidator> validators)
	{
		_validators = validators;
	}

	private void OnGuiInputPath()
	{
		EditorGUILayout.LabelField("Input path (json):");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.InputPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("Input path", "Assets", string.Empty);

				if (string.IsNullOrWhiteSpace(fullPath))
				{
					return;
				}

				Settings.InputPath = Path.GetRelativePath(Application.dataPath, fullPath);
				Save();
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

				if (string.IsNullOrWhiteSpace(fullPath))
				{
					return;
				}

				Settings.OutputPath = Path.GetRelativePath(Application.dataPath, fullPath) + @"\Configs.bytes";
				Save();
			}
		}
	}

	private void OnGuiCodeGenerationInputPath()
	{
		EditorGUILayout.LabelField("[Code Generate] Input path:");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.CodeGenerationInputPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("[Code Generate] Input Path", "Assets", string.Empty);

				if (string.IsNullOrWhiteSpace(fullPath))
				{
					return;
				}
				
				Settings.CodeGenerationInputPath = Path.GetRelativePath(Application.dataPath, fullPath);
				Save();
			}
		}
	}

	private void OnGuiCodeGenerationOutputPath()
	{
		EditorGUILayout.LabelField("[Code Generate] Output path:");

		using (new EditorHorizontalGroup())
		{
			GUI.enabled = false;
			EditorGUILayout.TextField(Settings.CodeGenerationOutputPath);
			GUI.enabled = true;

			if (GUILayout.Button("...", GUILayout.Width(30)))
			{
				var fullPath = EditorUtility.OpenFolderPanel("[Code Generate] Output Path", "Assets", string.Empty);

				if (string.IsNullOrWhiteSpace(fullPath))
				{
					return;
				}

				Settings.CodeGenerationOutputPath = Path.GetRelativePath(Application.dataPath, fullPath);
				Save();
			}
		}
	}

	private Task _codeGenerateTask;

	private void OnGuiButtons()
	{
		EditorGUILayout.Space();

		using (new EditorVerticalGroup())
		{
			using (new EditorDisabledGroup(_invokingCodeGen))
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
				new LinkDefinitionJsonConverter(),
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
		using (var fs = new FileStream(Settings.FullOutputPath, FileMode.OpenOrCreate))
		{
			//var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
			MessagePackSerializer.Serialize(fs, config);
		}

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
		_invokingCodeGen = true;

		new DefinitionCodeGenerator(typeof(T).GetTypeInfo(), Settings.FullCodeGenerationOutputPath)
			.Execute();

		new MessagePackCodeGenerator(Settings.CodeGenerationInputPath, Settings.CodeGenerationOutputPath)
			.Execute(() =>
			{
				_invokingCodeGen = false;
				_assetDbRefresh = true;
			});
	}

	#endregion
}

}