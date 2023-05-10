namespace EM.Configs.Editor
{

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assistant.Editor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

public sealed class DefinitionsAssistantCollection
{
	private readonly EditorWindow _window;

	private readonly Func<string, object, object> _changeValue;

	private readonly Action<object> _guiFields;

	private readonly AnimBool _showExtraFields = new(false);

	private readonly List<AnimBool> _showExtraFieldElements = new();

	private List<Type> _supportedTypes;

	private int _amountAdd;

	private int _addTypeIndex;

	private bool _isMaxFlag;

	private int _maxShow;

	private int _currentPage;

	private string _filter = string.Empty;

	#region DefinitionsCollection

	public DefinitionsAssistantCollection(EditorWindow window,
		Func<string, object, object> changeValue,
		Action<object> guiFields)
	{
		_window = window;
		_changeValue = changeValue;
		_guiFields = guiFields;

		_showExtraFields.valueChanged.AddListener(window.Repaint);
	}

	public void DoLayoutList(FieldInfo field,
		IList collection)
	{
		using var fadeGroup = new EditorFadeGroup(field.Name, _showExtraFields);

		if (!fadeGroup.IsVisible)
		{
			return;
		}

		FillExtraFieldElements(collection.Count);
		FillSupportedTypes(field);

		if (!CheckSupportedTypes())
		{
			return;
		}

		using (new EditorVerticalGroup(17))
		{
			OnGuiCollectionTopPanel(collection);

			if (!CheckCollectionCount(collection))
			{
				return;
			}

			OnGuiCollection(collection);
		}
	}

	private void OnGuiCollection(IList collection)
	{
		var count = GetCountItemsGivenFilter(collection);

		if (_isMaxFlag)
		{
			_maxShow = count;
		}

		if (count <= 0)
		{
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("There are no elements that match the filter", MessageType.Info);
			EditorGUILayout.Space();
			
			return;
		}

		OnGuiCollectionAllItem(collection, count);
	}

	private void OnGuiCollectionAllItem(IList collection, int count)
	{
		for (var i = (_currentPage - 1) * _maxShow;
		     i < Math.Clamp(_currentPage * _maxShow, 1, count);
		     i++)
		{
			OnGuiCollectionItem(collection, i);

			if (collection.Count <= 0)
			{
				break;
			}
		}
	}

	private bool CheckSupportedTypes()
	{
		var elementType = _supportedTypes[_addTypeIndex];

		if (!elementType.IsSubclassOf(typeof(ConfigLink)))
		{
			return true;
		}

		EditorGUILayout.HelpBox("Collections do not support Config Link", MessageType.Error);

		return false;
	}

	private static bool CheckCollectionCount(ICollection collection)
	{
		if (collection.Count > 0)
		{
			return true;
		}

		EditorGUILayout.HelpBox("collection is empty", MessageType.Info);

		return false;
	}

	private static void OnGuiCollectionPrimitiveElementButtons(IList collection,
		int index)
	{
		if (GUILayout.Button("-", GUILayout.Width(20)))
		{
			RemoveElement(collection, index);
		}

		if (GUILayout.Button("up", GUILayout.Width(30)))
		{
			UpMoveElement(collection, index);
		}

		if (GUILayout.Button("down", GUILayout.Width(50)))
		{
			DownMoveElement(collection, index);
		}
	}

	private static void RemoveElement(IList collection,
		int index)
	{
		collection.RemoveAt(index);
		EditorGUI.FocusTextInControl(null);
	}

	private static void UpMoveElement(IList collection,
		int index)
	{
		var newIndex = index + 1;

		if (newIndex >= collection.Count)
		{
			return;
		}

		MoveElement(collection, index, newIndex);
	}

	private static void DownMoveElement(IList collection,
		int index)
	{
		var newIndex = index - 1;

		if (newIndex < 0)
		{
			return;
		}

		MoveElement(collection, index, newIndex);
	}

	private static void MoveElement(IList collection,
		int index,
		int newIndex)
	{
		var item = collection[index];
		collection.RemoveAt(index);
		collection.Insert(newIndex, item);
		EditorGUI.FocusTextInControl(null);
	}

	private void FillExtraFieldElements(int count)
	{
		while (_showExtraFieldElements.Count < count)
		{
			var animBool = new AnimBool(true);
			animBool.valueChanged.AddListener(_window.Repaint);
			_showExtraFieldElements.Add(animBool);
		}
	}

	private void OnGuiCollectionItem(IList collection,
		int index)
	{
		if (TryGuiCollectionPrimitiveElement(collection, index))
		{
			return;
		}

		if (CheckFilter(collection, index))
		{
			OnGuiCollectionObjectItem(collection, index);
		}
	}

	private bool CheckFilter(IList collection,
		int index)
	{
		var item = collection[index];
		var primaryKey = GetPrimaryKey(item);
		var filter = _filter.ToLower();
		var result = primaryKey.ToLower().Contains(filter);

		return result;
	}

	private bool TryGuiCollectionPrimitiveElement(IList collection,
		int index)
	{
		var item = collection[index];
		var elementType = item.GetType();

		if (!elementType.IsPrimitive && elementType != typeof(string))
		{
			return false;
		}

		using (new EditorHorizontalGroup())
		{
			collection[index] = _changeValue.Invoke(string.Empty, item);
			OnGuiCollectionPrimitiveElementButtons(collection, index);
		}

		return true;
	}

	private void OnGuiCollectionObjectItem(IList collection,
		int index)
	{
		var item = collection[index];
		var title = GetGroupTitle(index, item);

		using (new EditorVerticalGroup(17, "GroupBox"))
		{
			OnGuiCollectionObjectElementTopPanel(collection, index);

			using (var fadeGroup = new EditorFadeGroup(title, _showExtraFieldElements[index]))
			{
				if (!fadeGroup.IsVisible)
				{
					return;
				}

				using (new EditorVerticalGroup(17))
				{
					_guiFields?.Invoke(item);
				}
			}
		}
	}

	private static string GetGroupTitle(int index,
		object item)
	{
		var primaryKey = GetPrimaryKey(item);
		var result = $"Index : {index} | PrimaryKey : \"{primaryKey}\"";

		return result;
	}

	private static string GetPrimaryKey(object item)
	{
		var primaryKey = "##";
		var fields = item.GetType().GetFields();

		foreach (var field in fields)
		{
			var primaryKeyAttribute = field.GetCustomAttribute<PrimaryKeyAttribute>();

			if (primaryKeyAttribute == null)
			{
				continue;
			}

			var value = field.GetValue(item);

			if (value != null)
			{
				primaryKey = value.ToString();
			}

			break;
		}

		return primaryKey;
	}

	private static void OnGuiCollectionObjectElementTopPanel(IList collection,
		int index)
	{
		using (new EditorHorizontalGroup(17))
		{
			if (GUILayout.Button("up", GUILayout.MaxWidth(80)))
			{
				UpMoveElement(collection, index);
			}

			if (GUILayout.Button("down", GUILayout.MaxWidth(80)))
			{
				DownMoveElement(collection, index);
			}

			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("remove", GUILayout.MaxWidth(80)))
			{
				RemoveElement(collection, index);
			}
		}
	}

	private void OnGuiCollectionTopPanel(IList collection)
	{
		using (new EditorVerticalGroup(17, "GroupBox"))
		{
			using (new EditorHorizontalGroup())
			{
				OnGuiCollectionAddPanel(collection);
				GUILayout.FlexibleSpace();
				OnGuiCollectionPagesPanel(collection);
				OnGuiCollectionMaxPanel();
			}

			EditorGUILayout.Space();
			EditorLayoutUtility.ToolbarSearch(ref _filter, 17);
		}
	}

	private void OnGuiCollectionAddPanel(IList collection)
	{
		_amountAdd = EditorGUILayout.IntField(_amountAdd, GUILayout.MaxWidth(50));
		_amountAdd = Math.Clamp(_amountAdd, 1, 5);
		OnGuiCollectionAddTypePanel();

		EditorGUILayout.Space(13);

		if (GUILayout.Button("Add"))
		{
			AddCollectionElement(collection);
			FillExtraFieldElements(collection.Count);
		}

		EditorGUILayout.LabelField($"Count : {collection.Count}", GUILayout.MaxWidth(100));
	}

	private void FillSupportedTypes(FieldInfo field)
	{
		if (_supportedTypes != null)
		{
			return;
		}

		var fieldType = field.FieldType;
		var type = fieldType.GenericTypeArguments[0];
		var unionAttributes = type.GetCustomAttributes<MessagePack.UnionAttribute>();
		_supportedTypes = unionAttributes.Select(unionAttribute => unionAttribute.SubType).ToList();

		if (!_supportedTypes.Any())
		{
			_supportedTypes.Add(type);
		}
	}

	private void OnGuiCollectionAddTypePanel()
	{
		var options = _supportedTypes.Select(type => type.Name).ToArray();
		_addTypeIndex = EditorGUILayout.Popup(_addTypeIndex, options);
	}

	private void OnGuiCollectionPagesPanel(IList collection)
	{
		if (collection.Count <= 0)
		{
			return;
		}

		if (GUILayout.Button("<"))
		{
			_currentPage--;
			EditorGUI.FocusTextInControl(null);
		}

		OnGuiCollectionPageCounter(collection);

		if (GUILayout.Button(">"))
		{
			_currentPage++;
			EditorGUI.FocusTextInControl(null);
		}
	}

	private void OnGuiCollectionPageCounter(IList collection)
	{
		_currentPage = EditorGUILayout.IntField(_currentPage, GUILayout.MaxWidth(50));

		var count = GetCountItemsGivenFilter(collection);
		
		if (count >= 1)
		{
			_maxShow = Math.Clamp(_maxShow, 1, count);
		}

		var ceilingCountPages = GetCeilingCountPages(count);

		if (ceilingCountPages <= 1)
		{
			ceilingCountPages = 1;
		}
		
		_currentPage = Math.Clamp(_currentPage, 1, ceilingCountPages);

		EditorGUILayout.LabelField($"/  {ceilingCountPages}", GUILayout.MaxWidth(60));
	}

	private int GetCountItemsGivenFilter(IList collection)
	{
		var count = 0;

		for (var i = 0; i < collection.Count; i++)
		{
			if (CheckFilter(collection, i))
			{
				count++;
			}
		}

		return count;
	}

	private int GetCeilingCountPages(int count)
	{
		var countPages = (float) count / _maxShow;
		var ceilingCountPages = (int) Math.Ceiling(countPages);

		return ceilingCountPages;
	}

	private void OnGuiCollectionMaxPanel()
	{
		var maxShowTemp = _maxShow;
		_maxShow = EditorGUILayout.IntField(_maxShow, GUILayout.MaxWidth(50));

		if (_isMaxFlag && maxShowTemp > _maxShow)
		{
			_isMaxFlag = false;
		}

		var buttonStyle = new GUIStyle(GUI.skin.button);
		var isMaxFlagTemp = _isMaxFlag;
		_isMaxFlag = GUILayout.Toggle(_isMaxFlag, "Max", buttonStyle);

		if (isMaxFlagTemp != _isMaxFlag)
		{
			EditorGUI.FocusTextInControl(null);
		}
	}

	private void AddCollectionElement(IList collection)
	{
		var elementType = _supportedTypes[_addTypeIndex];

		for (var i = 0; i < _amountAdd; i++)
		{
			CreateCollectionElement(elementType, collection);
		}
	}

	private static void CreateCollectionElement(Type elementType,
		IList collection)
	{
		if (elementType == typeof(int))
		{
			collection.Add(0);
		}
		else if (elementType == typeof(float))
		{
			collection.Add(0f);
		}
		else if (elementType == typeof(bool))
		{
			collection.Add(false);
		}
		else if (elementType == typeof(double))
		{
			collection.Add(0d);
		}
		else if (elementType == typeof(string))
		{
			collection.Add(string.Empty);
		}
		else if (elementType.IsClass)
		{
			var obj = Activator.CreateInstance(elementType);
			collection.Add(obj);
		}
		else
		{
			collection.Add(default);
		}
	}

	#endregion
}

}