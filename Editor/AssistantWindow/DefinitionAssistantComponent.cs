namespace EM.Configs.Editor
{

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EM.Assistant.Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public abstract class DefinitionAssistantComponent<T> : IAssistantComponent
	where T : class, new()
{
	private EditorWindow _window;

	private readonly IDefinitionAssistantHelper _externalHelper;

	private readonly T _definition;

	private readonly List<FieldInfo> _fields = new();

	private readonly Dictionary<string, DefinitionAssistantCollection> _collections = new();

	private readonly Dictionary<string, DefinitionAssistantObject> _objects = new();

	private readonly Dictionary<string, DefinitionAssistantLink> _links = new();
	
	private readonly Dictionary<string, DefinitionAssistantSpriteAtlas> _spriteAtlases = new();

	private bool _info;

	private bool _warning = true;

	#region IAssistantComponent

	public string Name { get; }

	public void Prepare(EditorWindow window)
	{
		_window = window;
		Reset();
	}

	public void OnGUI()
	{
		OnGuiTopPanel();

		if (_definition == null)
		{
			return;
		}

		OnGuiFields(_definition);
	}

	#endregion

	#region DefinitionsAssistantComponent

	protected DefinitionAssistantComponent(IDefinitionAssistantHelper externalHelper,
		string name,
		T definition,
		params string[] fieldNames)
	{
		Name = name;
		_externalHelper = externalHelper;
		_definition = definition;

		FieldFields(fieldNames);
	}

	protected abstract string DirectoryPath { get; }

	private string FileName => $"{Name}.json";

	private void OnGuiTopPanel()
	{
		using (new EditorVerticalGroup(17, "GroupBox"))
		{
			OnGuiCreateButton();
			OnGuiButtons();
		}
	}

	private void FieldFields(IEnumerable<string> fieldNames)
	{
		var type = typeof(T);

		foreach (var fieldName in fieldNames)
		{
			var field = type.GetField(fieldName);

			if (field != null)
			{
				_fields.Add(field);
			}
		}
	}

	private void OnGuiCreateButton()
	{
		if (_definition == null)
		{
			EditorGUILayout.HelpBox("File not found. Create a new object.", MessageType.Warning);
		}

		OnGuiOutputPath();
	}

	private void OnGuiOutputPath()
	{
		using (new EditorHorizontalGroup())
		{
			var filePath = DirectoryPath + FileName;
			GUI.enabled = false;
			EditorGUILayout.TextField("Output path", filePath);
			GUI.enabled = true;

			if (GUILayout.Button("Show in Explorer", GUILayout.Width(120)))
			{
				var fullPath = Path.GetFullPath(filePath);
				ShowInExplorer(fullPath);
			}
		}
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

	private void OnGuiButtons()
	{
		if (_definition == null)
		{
			return;
		}

		using (new EditorHorizontalGroup(17))
		{
			if (GUILayout.Button("Save ⇪", GUILayout.MaxWidth(70)))
			{
				Save();
			}

			if (GUILayout.Button("Reset ↺", GUILayout.MaxWidth(70)))
			{
				Reset();
				EditorGUI.FocusTextInControl(null);
			}

			var buttonStyle = new GUIStyle(GUI.skin.button);
			_info = GUILayout.Toggle(_info, "Info ⓘ", buttonStyle, GUILayout.MaxWidth(80));
			_warning = GUILayout.Toggle(_warning, "Warning ⓦ", buttonStyle, GUILayout.MaxWidth(80));
		}
	}

	private void Save()
	{
		if (_definition == null)
		{
			return;
		}

		try
		{
			var json = SerializeDefinitions();
			var filePath = DirectoryPath + FileName;
			using var outputFile = new StreamWriter(filePath, false, Encoding.UTF8);
			outputFile.Write(json);
		}
		catch (IOException e)
		{
			Debug.LogError("File write error: " + e.Message);
		}
	}

	private string SerializeDefinitions()
	{
		JsonSerializerSettings jsonSettings = new()
		{
			Converters =
			{
				new LinkDefinitionJsonConverter()
			},
			Formatting = Formatting.Indented
		};

		var json = JsonConvert.SerializeObject(_definition, jsonSettings);
		var jObject = JObject.Parse(json);
		var removeList = new List<string>();

		foreach (var keyValuePair in jObject)
		{
			if (_fields.All(field => field.Name != keyValuePair.Key))
			{
				removeList.Add(keyValuePair.Key);
			}
		}

		foreach (var key in removeList)
		{
			jObject.Remove(key);
		}

		return jObject.ToString();
	}

	private void Reset()
	{
		var filePath = DirectoryPath + FileName;

		if (!File.Exists(filePath))
		{
			return;
		}

		JsonSerializerSettings jsonSettings = new()
		{
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			Converters =
			{
				new LinkDefinitionJsonConverter(),
				new UnionConverter()
			}
		};

		try
		{
			using var streamReader = new StreamReader(filePath);
			var json = streamReader.ReadToEnd();
			JsonConvert.PopulateObject(json, _definition, jsonSettings);
		}
		catch (IOException e)
		{
			Debug.LogError("File read error: " + e.Message);
		}
	}

	private void OnGuiFields(object instance)
	{
		var fields = GetFields(instance);

		foreach (var field in fields)
		{
			if (CheckInstanceAndFields(instance, field))
			{
				continue;
			}

			if (CheckSpriteAtlas(instance, field))
			{
				continue;
			}

			if (CheckString(instance, field))
			{
				continue;
			}

			if (CheckCollection(instance, field))
			{
				continue;
			}

			if (CheckLink(instance, field))
			{
				continue;
			}

			if (CheckClass(instance, field))
			{
				continue;
			}

			if (!CheckBaseType(instance, field))
			{
				EditorGUILayout.HelpBox($"Type {field.FieldType} not supported!", MessageType.Warning);
			}
		}
	}

	private bool CheckInstanceAndFields(object instance,
		FieldInfo field)
	{
		if (instance != _definition)
		{
			return false;
		}

		if (_fields.All(f => f.Name != field.Name))
		{
			return true;
		}

		return false;
	}

	private static IEnumerable<FieldInfo> GetFields(object instance)
	{
		if (instance == null)
		{
			return new List<FieldInfo>();
		}
		
		var type = instance.GetType();
		var baseType = type.BaseType;
		var fields = type.GetFields();

		if (baseType == null || baseType == typeof(object))
		{
			return fields;
		}

		var baseFields = baseType.GetFields();
		var baseTypeSet = new HashSet<string>(baseFields.Select(f => f.Name));
		var additionalFields = fields.Where(f => !baseTypeSet.Contains(f.Name));
		var resultList = new List<FieldInfo>(baseFields);
		resultList.AddRange(additionalFields);

		return resultList;
	}

	private bool CheckSpriteAtlas(object instance,
		FieldInfo field)
	{
		var fieldType = field.FieldType;
		var fieldValue = field.GetValue(instance);

		if (fieldType != typeof(SpriteAtlasDefinition))
		{
			return false;
		}
		
		fieldValue ??= Activator.CreateInstance(fieldType);
		var spriteAtlas = GetSpriteAtlas(field, fieldValue);
		var useGroup = instance == _definition;
		spriteAtlas.DoLayoutSpriteAtlas(field, fieldValue, useGroup);
		field.SetValue(instance, fieldValue);

		return true;
	}

	private static bool CheckString(object instance,
		FieldInfo field)
	{
		var fieldType = field.FieldType;
		var fieldValue = field.GetValue(instance);

		if (fieldType != typeof(string))
		{
			return false;
		}

		var value = (string) fieldValue ?? string.Empty;
		value = EditorGUILayout.TextField(field.Name, value);
		field.SetValue(instance, value);

		return true;
	}

	private bool CheckCollection(object instance,
		FieldInfo field)
	{
		var fieldValue = field.GetValue(instance);

		if (fieldValue is not IList collection)
		{
			return false;
		}

		var collectionGui = GetCollection(field, collection);
		var useGroup = instance == _definition;
		collectionGui.DoLayoutList(field, collection, useGroup);
		field.SetValue(instance, collection);

		return true;
	}

	private bool CheckLink(object instance,
		FieldInfo field)
	{
		var fieldType = field.FieldType;
		var fieldValue = field.GetValue(instance);

		if (!fieldType.IsSubclassOf(typeof(LinkDefinition)))
		{
			return false;
		}

		fieldValue ??= Activator.CreateInstance(fieldType);
		var link = GetLink(field, fieldValue);
		link.DoLayoutLink(_definition, field, fieldValue);
		field.SetValue(instance, fieldValue);

		return true;
	}

	private bool CheckClass(object instance,
		FieldInfo field)
	{
		var fieldType = field.FieldType;

		if (!fieldType.IsClass)
		{
			return false;
		}

		var obj = GetObject(instance, field);
		var useGroup = instance == _definition;
		obj.DoLayoutObject(instance, field, useGroup);

		return true;
	}

	private static bool CheckBaseType(object instance,
		FieldInfo field)
	{
		if (!field.FieldType.IsPrimitive)
		{
			return false;
		}

		var obj = field.GetValue(instance);
		var value = ChangeValue(field.Name, value: obj);
		field.SetValue(instance, value);

		return true;
	}

	private static object ChangeValue(string name,
		object value)
	{
		return value switch
		{
			string sValue => EditorGUILayout.TextField(name, sValue),
			bool sValue => EditorGUILayout.Toggle(name, sValue),
			int sValue => EditorGUILayout.IntField(name, sValue),
			float sValue => EditorGUILayout.FloatField(name, sValue),
			double sValue => EditorGUILayout.DoubleField(name, sValue),
			_ => false
		};
	}

	private DefinitionAssistantCollection GetCollection(FieldInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_collections.TryGetValue(key, out var collection))
		{
			return collection;
		}

		collection = new DefinitionAssistantCollection(_window, ChangeValue, OnGuiFields);
		_collections.Add(key, collection);

		return collection;
	}

	private DefinitionAssistantObject GetObject(object instance,
		FieldInfo field)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_objects.TryGetValue(key, out var resultObject))
		{
			return resultObject;
		}

		resultObject = new DefinitionAssistantObject(_window, OnGuiFields);
		_objects.Add(key, resultObject);

		return resultObject;
	}

	private DefinitionAssistantLink GetLink(FieldInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_links.TryGetValue(key, out var resultLink))
		{
			return resultLink;
		}

		resultLink = new DefinitionAssistantLink(_externalHelper);
		_links.Add(key, resultLink);

		return resultLink;
	}

	private DefinitionAssistantSpriteAtlas GetSpriteAtlas(FieldInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_spriteAtlases.TryGetValue(key, out var resultSpriteAtlas))
		{
			return resultSpriteAtlas;
		}
		
		resultSpriteAtlas = new DefinitionAssistantSpriteAtlas(_window);
		_spriteAtlases.Add(key, resultSpriteAtlas);

		return resultSpriteAtlas;
	}

	#endregion
}

}