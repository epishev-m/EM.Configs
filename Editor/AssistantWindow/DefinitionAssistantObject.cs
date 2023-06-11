namespace EM.Configs.Editor
{

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant.Editor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

public sealed class DefinitionAssistantObject
{
	private readonly Action<object> _guiFields;

	private readonly AnimBool _showExtraFields = new(false);

	private List<Type> _supportedTypes;

	private int _addTypeIndex;

	#region DefinitionsObject

	public DefinitionAssistantObject(EditorWindow window,
		Action<object> guiFields)
	{
		_guiFields = guiFields;
		_showExtraFields.valueChanged.AddListener(window.Repaint);
	}

	public void DoLayoutObject(object instance,
		FieldInfo field,
		bool useGroup)
	{
		if (!useGroup)
		{
			using var fadeGroup = new EditorFadeGroup(field.Name, _showExtraFields);

			if (!fadeGroup.IsVisible)
			{
				return;
			}

			OnGuiTopPanel(instance, field);

			using (new EditorVerticalGroup(17))
			{
				OnGuiObject(instance, field);
			}
		}
		else
		{
			OnGuiTopPanel(instance, field);

			using (new EditorVerticalGroup(17, "GroupBox"))
			{
				OnGuiObject(instance, field);
			}
		}
	}

	private void OnGuiObject(object instance,
		FieldInfo field)
	{
		var fieldValue = GetValue(instance, field);
		_guiFields.Invoke(fieldValue);
	}

	private void FillSupportedTypes(FieldInfo field)
	{
		if (_supportedTypes != null)
		{
			return;
		}

		var type = field.FieldType;
		var unionAttributes = type.GetCustomAttributes<MessagePack.UnionAttribute>();
		_supportedTypes = unionAttributes.Select(unionAttribute => unionAttribute.SubType).ToList();

		if (!_supportedTypes.Any())
		{
			_supportedTypes.Add(type);
		}
	}

	private void OnGuiTopPanel(object instance,
		FieldInfo field)
	{
		FillSupportedTypes(field);

		if (_supportedTypes.Count <= 1)
		{
			return;
		}

		using (new EditorVerticalGroup(17, "GroupBox"))
		{
			OnGUiCurrentType(instance, field);
			OnGuiCreatePanel(instance, field);
		}
	}

	private void OnGuiCreatePanel(object instance,
		FieldInfo field)
	{
		using (new EditorHorizontalGroup())
		{
			var options = _supportedTypes.Select(type => type.Name).ToArray();
			_addTypeIndex = EditorGUILayout.Popup(_addTypeIndex, options);

			EditorGUILayout.Space(13);

			if (GUILayout.Button("Create"))
			{
				CreateValue(instance, field);
			}

			GUILayout.FlexibleSpace();
		}
	}

	private static void OnGUiCurrentType(object instance,
		FieldInfo fieldInfo)
	{
		var value = fieldInfo.GetValue(instance);

		if (value == null)
		{
			return;
		}

		GUI.enabled = false;
		EditorGUILayout.TextField("Current type:", value.GetType().Name);
		GUI.enabled = true;
	}

	private object GetValue(object instance,
		FieldInfo field)
	{
		var value = field.GetValue(instance) ?? CreateValue(instance, field);

		return value;
	}

	private object CreateValue(object instance,
		FieldInfo field)
	{
		var type = _supportedTypes[_addTypeIndex];
		var value = Activator.CreateInstance(type);
		field.SetValue(instance, value);

		return value;
	}

	#endregion
}

}