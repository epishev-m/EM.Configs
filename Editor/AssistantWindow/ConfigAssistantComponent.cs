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

public abstract class ConfigAssistantComponent<T> : IAssistantComponent
	where T : class, new()
{
	private EditorWindow _window;

	private readonly IConfigAssistantHelper _externalHelper;

	private readonly T _config;

	private readonly List<FieldInfo> _fields = new();

	private readonly Dictionary<string, ConfigAssistantCollection> _collections = new();

	private readonly Dictionary<string, ConfigAssistantObject> _objects = new();

	private readonly Dictionary<string, ConfigAssistantLink> _links = new();
	
	private readonly Dictionary<string, ConfigAssistantSpriteAtlas> _spriteAtlases = new();

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

		if (_config == null)
		{
			return;
		}

		OnGuiFields(_config);
	}

	#endregion

	#region ConfigAssistantComponent

	protected ConfigAssistantComponent(IConfigAssistantHelper externalHelper,
		string name,
		T config,
		params string[] fieldNames)
	{
		Name = name;
		_externalHelper = externalHelper;
		_config = config;

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
		if (_config == null)
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
		if (_config == null)
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
		if (_config == null)
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
				new ConfigLinkJsonConverter(),
				new ColorJsonConverter()
			},
			Formatting = Formatting.Indented
		};

		var json = JsonConvert.SerializeObject(_config, jsonSettings);
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
				new ConfigLinkJsonConverter(),
				new ColorJsonConverter(),
				new UnionConverter()
			}
		};

		try
		{
			using var streamReader = new StreamReader(filePath);
			var json = streamReader.ReadToEnd();
			JsonConvert.PopulateObject(json, _config, jsonSettings);
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

			if (CheckUnityBaseType(instance, field))
			{
				continue;
			}

			if (CheckClass(instance, field))
			{
				continue;
			}

			if (CheckEnum(instance, field))
			{
				continue;
			}

			if (!CheckBaseType(instance, field))
			{
				EditorGUILayout.HelpBox($"Type {field.GetValueType()} not supported!", MessageType.Warning);
			}
		}
	}

	private bool CheckInstanceAndFields(object instance,
		MemberInfo field)
	{
		if (instance != _config)
		{
			return false;
		}

		if (_fields.All(f => f.Name != field.Name))
		{
			return true;
		}

		return false;
	}

	private static IEnumerable<MemberInfo> GetFields(object instance)
	{
		var resultList = new List<MemberInfo>();

		if (instance == null)
		{
			return resultList;
		}

		var type = instance.GetType();
		var baseType = type.BaseType;
		var fields = type.GetFields();
		var properties = type.GetProperties();

		if (baseType == null || baseType == typeof(object))
		{
			resultList.AddRange(fields);
			resultList.AddRange(properties);

			return fields;
		}

		var baseFields = baseType.GetFields();
		var baseFieldsSet = new HashSet<string>(baseFields.Select(f => f.Name));
		var additionalFields = fields.Where(f => !baseFieldsSet.Contains(f.Name));

		var baseProperties = baseType.GetProperties();

		resultList.AddRange(baseFields);
		resultList.AddRange(baseProperties);
		resultList.AddRange(additionalFields);

		return resultList;
	}

	private bool CheckSpriteAtlas(object instance,
		MemberInfo field)
	{
		var fieldType = field.GetValueType();

		if (!typeof(ISpriteAtlas).IsAssignableFrom(fieldType))
		{
			return false;
		}

		var fieldValue = field.GetValue(instance);
		fieldValue ??= Activator.CreateInstance(fieldType);
		var spriteAtlas = GetSpriteAtlas(field, fieldValue);
		var useGroup = instance == _config;
		spriteAtlas.DoLayoutSpriteAtlas(field, fieldValue, useGroup);
		field.SetValue(instance, fieldValue);

		return true;
	}

	private static bool CheckString(object instance,
		MemberInfo field)
	{
		var fieldType = field.GetValueType();
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
		MemberInfo field)
	{
		var fieldValue = field.GetValue(instance);

		if (fieldValue is not IList collection)
		{
			return false;
		}

		var collectionGui = GetCollection(field, collection);
		var useGroup = instance == _config;
		collectionGui.DoLayoutList(field, collection, useGroup);
		field.SetValue(instance, collection);

		return true;
	}

	private bool CheckLink(object instance,
		MemberInfo field)
	{
		var fieldType = field.GetValueType();
		var fieldValue = field.GetValue(instance);

		if (!fieldType.IsSubclassOf(typeof(LinkConfig)))
		{
			return false;
		}

		fieldValue ??= Activator.CreateInstance(fieldType);
		var link = GetLink(field, fieldValue);
		link.DoLayoutLink(field, fieldValue);
		field.SetValue(instance, fieldValue);

		return true;
	}

	private bool CheckClass(object instance,
		MemberInfo field)
	{
		var fieldType = field.GetValueType();

		if (!fieldType.IsClass)
		{
			return false;
		}

		var obj = GetObject(instance, field);
		var useGroup = instance == _config;
		obj.DoLayoutObject(instance, field, useGroup);

		return true;
	}

	private static bool CheckEnum(object instance,
		MemberInfo field)
	{
		var fieldType = field.GetValueType();

		if (!fieldType.IsEnum)
		{
			return false;
		}

		var options = Enum.GetNames(fieldType);
		var value = field.GetValue(instance);
		value = EditorGUILayout.Popup(field.Name, (int) value, options.ToArray());
		field.SetValue(instance, value);

		return true;
	}

	private static bool CheckUnityBaseType(object instance,
		MemberInfo field)
	{
		var obj = field.GetValue(instance);

		switch (obj)
		{
			case Color color:
			{
				var value = EditorGUILayout.ColorField(field.Name, color);
				field.SetValue(instance, value);

				return true;
			}
			case Vector2 vector2:
			{
				var value = EditorGUILayout.Vector2Field(field.Name, vector2);
				field.SetValue(instance, value);

				return true;
			}
			case Vector3 vector3:
			{
				var value = EditorGUILayout.Vector3Field(field.Name, vector3);
				field.SetValue(instance, value);

				return true;
			}
		}

		return false;
	}
	
	private static object ChangeUnityBaseValue(string name,
		object value)
	{
		return value switch
		{
			string sValue => EditorGUILayout.TextField(name, sValue),
			bool sValue => EditorGUILayout.Toggle(name, sValue),
			int sValue => EditorGUILayout.IntField(name, sValue),
			long sValue => EditorGUILayout.LongField(name, sValue),
			float sValue => EditorGUILayout.FloatField(name, sValue),
			double sValue => EditorGUILayout.DoubleField(name, sValue),
			_ => throw new InvalidOperationException()
		};
	}

	private static bool CheckBaseType(object instance,
		MemberInfo field)
	{
		if (!field.GetValueType().IsPrimitive)
		{
			return false;
		}

		var obj = field.GetValue(instance);
		var value = ChangeValue(field.Name, obj);
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
			long sValue => EditorGUILayout.LongField(name, sValue),
			float sValue => EditorGUILayout.FloatField(name, sValue),
			double sValue => EditorGUILayout.DoubleField(name, sValue),
			_ => throw new InvalidOperationException()
		};
	}

	private ConfigAssistantCollection GetCollection(MemberInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_collections.TryGetValue(key, out var collection))
		{
			return collection;
		}

		collection = new ConfigAssistantCollection(_window, ChangeValue, OnGuiFields);
		_collections.Add(key, collection);

		return collection;
	}

	private ConfigAssistantObject GetObject(object instance,
		MemberInfo field)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_objects.TryGetValue(key, out var resultObject))
		{
			return resultObject;
		}

		resultObject = new ConfigAssistantObject(_window, OnGuiFields);
		_objects.Add(key, resultObject);

		return resultObject;
	}

	private ConfigAssistantLink GetLink(MemberInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_links.TryGetValue(key, out var resultLink))
		{
			return resultLink;
		}

		resultLink = new ConfigAssistantLink(_externalHelper);
		_links.Add(key, resultLink);

		return resultLink;
	}

	private ConfigAssistantSpriteAtlas GetSpriteAtlas(MemberInfo field,
		object instance)
	{
		var fieldHashCode = field.GetHashCode();
		var instanceHashCode = instance.GetHashCode();
		var key = $"{fieldHashCode}{instanceHashCode}";

		if (_spriteAtlases.TryGetValue(key, out var resultSpriteAtlas))
		{
			return resultSpriteAtlas;
		}
		
		resultSpriteAtlas = new ConfigAssistantSpriteAtlas(_window);
		_spriteAtlases.Add(key, resultSpriteAtlas);

		return resultSpriteAtlas;
	}

	#endregion
}

}