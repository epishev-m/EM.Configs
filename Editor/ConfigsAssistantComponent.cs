using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EM.Assistant.Editor;
using EM.Foundation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.UIElements;

namespace EM.Configs.Editor
{

public abstract class ConfigsAssistantComponent<T> : AssistantComponent
	where T : class, new()
{
	private IConfigsSerializerFactory _sourceSerializerFactory;

	private IConfigsSerializerFactory _releaseSerializerFactory;

	private IConfigsValidatorFactory _validatorFactory;

	private ConfigsProjectSettingsVisualElement _projectSettingsVisualElement;

	private TextField _messagePackPathTextField;

	private VisualElement _messagePack;

	private TextField _inputPathTextField;

	private VisualElement _inputPath;

	private VisualElement _outputPath;

	private TextField _outputPathTextField;

	private VisualElement _version;

	private DropdownField _versionDropdownField;

	private VisualElement _controlButtons;

	#region AssistantComponent

	protected override void OnInitialized()
	{
		base.OnInitialized();
		CreateFactories();
		CreateProjectSettingsPanel();
	}

	protected override void OnComposed()
	{
		base.OnComposed();
		Root.AddChild(_projectSettingsVisualElement);

		if (_projectSettingsVisualElement.Settings != null)
		{
			OnProjectSettingsVisualElementCreated();
		}
	}

	#endregion

	#region ConfigsAssistantComponent

	protected abstract IConfigsSerializerFactory GetSourceSerializerFactory();

	protected abstract IConfigsSerializerFactory GetReleaseSerializerFactory();

	protected abstract IConfigsValidatorFactory GetValidatorFactory();

	private void CreateFactories()
	{
		_sourceSerializerFactory = GetSourceSerializerFactory();
		_releaseSerializerFactory = GetReleaseSerializerFactory();
		_validatorFactory = GetValidatorFactory();
	}

	private void CreateProjectSettingsPanel()
	{
		_projectSettingsVisualElement = new ConfigsProjectSettingsVisualElement()
			.SetStyleMargin(10, 0, 0, 0);
		_projectSettingsVisualElement.OnCreated += OnProjectSettingsVisualElementCreated;
	}

	private void CreateMessagePackCodeGenFilePath()
	{
		if (_projectSettingsVisualElement.Settings == null)
		{
			return;
		}

		_messagePackPathTextField = new TextField("Message Pack:")
			.SetStyleFlexBasisPercent(75)
			.AddValueChangedCallback<TextField, string>(OnMessagePackValueChanged)
			.SetValue(_projectSettingsVisualElement.Settings.MessagePackPath);

		var button = new Button(OnMessagePackFilePathButtonClicked)
			.SetStyleFlexBasisPercent(5)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("...");

		var showInExplorerButton = new Button(() =>
			{
				ShowInExplorer(_projectSettingsVisualElement.Settings.MessagePackPath);
			})
			.SetStyleFlexBasisPercent(20)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("Show in Explorer");

		_messagePack = new VisualElement()
			.SetStyleFlexDirection(FlexDirection.Row)
			.SetStyleJustifyContent(Justify.SpaceAround)
			.AddChild(_messagePackPathTextField)
			.AddChild(button)
			.AddChild(showInExplorerButton);
	}

	private void CreateInputPath()
	{
		if (_projectSettingsVisualElement.Settings == null)
		{
			return;
		}

		_inputPathTextField = new TextField("Input path (source):")
			.SetStyleFlexBasisPercent(75)
			.AddValueChangedCallback<TextField, string>(OnInputValueChanged)
			.SetValue(_projectSettingsVisualElement.Settings.InputPath);

		var button = new Button(OnInputPathButtonClicked)
			.SetStyleFlexBasisPercent(5)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("...");

		var showInExplorerButton = new Button(() =>
			{
				ShowInExplorer(_projectSettingsVisualElement.Settings.InputPath);
			})
			.SetStyleFlexBasisPercent(20)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("Show in Explorer");

		_inputPath = new VisualElement()
			.SetStyleFlexDirection(FlexDirection.Row)
			.SetStyleJustifyContent(Justify.SpaceAround)
			.AddChild(_inputPathTextField)
			.AddChild(button)
			.AddChild(showInExplorerButton);
	}

	private void CreateOutputPath()
	{
		if (_projectSettingsVisualElement.Settings == null)
		{
			return;
		}

		_outputPathTextField = new TextField("Output path:")
			.SetStyleFlexBasisPercent(75)
			.AddValueChangedCallback<TextField, string>(OnOutputValueChanged)
			.SetValue(_projectSettingsVisualElement.Settings.OutputPath);

		var button = new Button(OnOutputPathButtonClicked)
			.SetStyleFlexBasisPercent(5)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("...");

		var showInExplorerButton = new Button(() =>
			{
				SelectInProject(_projectSettingsVisualElement.Settings.OutputPath);
			})
			.SetStyleFlexBasisPercent(20)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("Select in Project");

		_outputPath = new VisualElement()
			.SetStyleFlexDirection(FlexDirection.Row)
			.SetStyleJustifyContent(Justify.SpaceAround)
			.AddChild(_outputPathTextField)
			.AddChild(button)
			.AddChild(showInExplorerButton);
	}

	private void CreateVersion()
	{
		var folders = Directory.GetDirectories(_projectSettingsVisualElement.Settings.InputPath, "v*")
			.Select(Path.GetFileName)
			.ToList();
		folders.Insert(0, "none");

		_versionDropdownField = new DropdownField("Version:", folders, 0)
			.SetStyleFlexBasisPercent(75)
			.AddValueChangedCallback<DropdownField, string>(OnVersionValueChanged)
			.SetValue(_projectSettingsVisualElement.Settings.Version);

		var button = new Button(OnVersionUpdateButtonClicked)
			.SetStyleFlexBasisPercent(25)
			.SetStyleMargin(0, 0, 10, 0)
			.SetText("Update version list");

		_version = new VisualElement()
			.SetStyleFlexDirection(FlexDirection.Row)
			.SetStyleJustifyContent(Justify.SpaceAround)
			.AddChild(_versionDropdownField)
			.AddChild(button);
	}

	private void CreateButtons()
	{
		var configureButton = new Button(OnConfigureButtonClicked)
			.SetStyleFlexBasisPercent(50)
			.SetText("Configure");

		var validateButton = new Button(OnValidateButtonClicked)
			.SetStyleFlexBasisPercent(50)
			.SetText("Validate");

		_controlButtons = new VisualElement()
			.SetStyleFlexDirection(FlexDirection.Row)
			.SetStyleJustifyContent(Justify.SpaceAround)
			.SetStyleMargin(0, 10, 0, 0)
			.AddChild(validateButton)
			.AddChild(configureButton);
	}

	private static void SelectInProject(string path)
	{
		var asset = AssetDatabase.LoadMainAssetAtPath(path);
		Selection.activeObject = asset;
	}

	private static void ShowInExplorer(string path)
	{
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
				System.Diagnostics.Process.Start("explorer.exe", "/select," + path.Replace('/', '\\'));
				break;
			case RuntimePlatform.OSXEditor:
				System.Diagnostics.Process.Start("open", "-R " + path);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void SetAddressableFlag()
	{
		AssetDatabase.Refresh();
		var guiID = AssetDatabase.AssetPathToGUID(_projectSettingsVisualElement.Settings.OutputPath);
		var settings = AddressableAssetSettingsDefaultObject.Settings;
		var assetEntry = settings.CreateOrMoveEntry(guiID, settings.DefaultGroup);
		assetEntry.address = Name;
	}

	private string GetInputPath()
	{
		var path = new StringBuilder(_projectSettingsVisualElement.Settings.InputPath)
			.Append("\\");

		if (_projectSettingsVisualElement.Settings.Version != "none")
		{
			path.Append(_projectSettingsVisualElement.Settings.Version);
		}

		return path.ToString();
	}

	private void OnProjectSettingsVisualElementCreated()
	{
		CreateMessagePackCodeGenFilePath();
		CreateInputPath();
		CreateOutputPath();
		CreateVersion();
		CreateButtons();

		Root.AddChild(_messagePack)
			.AddChild(_inputPath)
			.AddChild(_outputPath)
			.AddChild(_version)
			.AddChild(_controlButtons);
	}

	private void OnMessagePackValueChanged(string value)
	{
		_messagePackPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.MessagePackPath);
	}

	private void OnMessagePackFilePathButtonClicked()
	{
		var fullPath = EditorUtility.OpenFilePanel("MessagePack code gen file.bat", "", "bat");

		if (string.IsNullOrWhiteSpace(fullPath))
		{
			return;
		}

		_projectSettingsVisualElement.Settings.MessagePackPath =
			Path.GetRelativePath(Application.dataPath + "/..", fullPath);
		_messagePackPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.MessagePackPath);
		_projectSettingsVisualElement.Save();
	}

	private void OnInputValueChanged(string value)
	{
		_inputPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.InputPath);
	}

	private void OnInputPathButtonClicked()
	{
		var fullPath = EditorUtility.OpenFolderPanel("Input path", "Assets", string.Empty);

		if (string.IsNullOrWhiteSpace(fullPath))
		{
			return;
		}

		_projectSettingsVisualElement.Settings.InputPath = Path.GetRelativePath(Application.dataPath + "/..", fullPath);
		_inputPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.InputPath);
		_projectSettingsVisualElement.Save();
	}

	private void OnOutputValueChanged(string value)
	{
		_outputPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.OutputPath);
	}

	private void OnOutputPathButtonClicked()
	{
		var fullPath = EditorUtility.OpenFolderPanel("Output path", "Assets", "");

		if (string.IsNullOrWhiteSpace(fullPath))
		{
			return;
		}

		var relativePath = Path.GetRelativePath(Application.dataPath + "/..", fullPath);
		_projectSettingsVisualElement.Settings.OutputPath = $"{relativePath}\\{Name}.bytes";
		_outputPathTextField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.OutputPath);
		_projectSettingsVisualElement.Save();
	}

	private void OnVersionValueChanged(string value)
	{
		_projectSettingsVisualElement.Settings.Version = value;
		_projectSettingsVisualElement.Save();
		_versionDropdownField.labelElement.style.backgroundColor = new StyleColor(Color.clear);
	}

	private void OnVersionUpdateButtonClicked()
	{
		var folders = Directory.GetDirectories(_projectSettingsVisualElement.Settings.InputPath, "v*")
			.Select(Path.GetFileName)
			.ToList();
		folders.Insert(0, "none");

		_versionDropdownField.choices = folders;

		if (folders.All(s => s != _projectSettingsVisualElement.Settings.Version))
		{
			_versionDropdownField.labelElement.style.backgroundColor = new StyleColor(Color.red);
		}

		_versionDropdownField.SetValueWithoutNotify(_projectSettingsVisualElement.Settings.Version);
	}

	private void OnConfigureButtonClicked()
	{
		var sourceCatalog = new LibraryEntryCatalog();
		var sourceSerializer = _sourceSerializerFactory.Create(sourceCatalog);
		var path = GetInputPath();
		var deserializeResult = sourceSerializer.Deserialize<T>(path);

		if (deserializeResult.Failure)
		{
			if (deserializeResult is ErrorResult<T> deserializeErrorResult)
			{
				Debug.LogError(deserializeErrorResult.Message);
			}
		}

		Result result;
		var releaseCatalog = new LibraryEntryCatalog();
		var releaseSerializer = _releaseSerializerFactory.Create(releaseCatalog);
		using (var fs = File.Create(_projectSettingsVisualElement.Settings.OutputPath))
		{
			result = releaseSerializer.Serialize(fs, deserializeResult.Data);
		}

		if (result.Success)
		{
			SetAddressableFlag();
			Debug.Log("Configure - SUCCESS");
			return;
		}

		if (result is ErrorResult serializerErrorResult)
		{
			Debug.LogError(serializerErrorResult.Message);
		}
	}

	private void OnValidateButtonClicked()
	{
		var catalog = new LibraryEntryCatalog();
		var serializer = _sourceSerializerFactory.Create(catalog);
		var path = GetInputPath();
		var result = serializer.Deserialize<T>(path);

		if (result is ErrorResult<T> deserializeErrorResult)
		{
			Debug.LogError(deserializeErrorResult.Message);

			return;
		}

		catalog.Initialize();
		var validator = _validatorFactory.Create(catalog);
		var stringBuilder = new StringBuilder();
		var resultList = new List<ValidationResult>();

		if (!validator.TryValidate(result.Data, resultList))
		{
			if (resultList.Count > 0)
			{
				stringBuilder.AppendLine("Validation failed!")
					.AppendLine();
			}

			foreach (var validationResult in resultList)
			{
				stringBuilder.AppendLine(validationResult.ToString());
			}
		}

		if (stringBuilder.Length > 0)
		{
			Debug.LogError(stringBuilder.ToString());
		}
		else
		{
			Debug.Log("Validate - SUCCESS");
		}
	}

	#endregion
}

}