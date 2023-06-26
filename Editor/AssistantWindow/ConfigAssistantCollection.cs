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

public sealed class ConfigAssistantCollection
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

	public ConfigAssistantCollection(EditorWindow window,
		Func<string, object, object> changeValue,
		Action<object> guiFields)
	{
		_window = window;
		_changeValue = changeValue;
		_guiFields = guiFields;

		_showExtraFields.valueChanged.AddListener(window.Repaint);
	}

	public void DoLayoutList(MemberInfo field,
		IList collection,
		bool useGroup)
	{
		if (!useGroup)
		{
			using var fadeGroup = new EditorFadeGroup(field.Name, _showExtraFields);

			if (!fadeGroup.IsVisible)
			{
				return;
			}

			OnGuiCollection(field, collection);
		}
		else
		{
			OnGuiCollection(field, collection);
		}
	}

	private void OnGuiCollection(MemberInfo field,
		IList collection)
	{
		FillExtraFieldElements(collection.Count);
		FillSupportedTypes(field.GetValueType());

		if (!CheckSupportedTypes())
		{
			return;
		}

		OnGuiCollectionTopPanel(collection);

		if (!CheckCollectionCount(collection))
		{
			return;
		}

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

		OnGuiCollectionAllItem(collection);
	}

	private void OnGuiCollectionAllItem(IList collection)
	{
		for (var i = (_currentPage - 1) * _maxShow;
		     i <_currentPage * _maxShow;
		     i++)
		{
			if (TryGuiCollectionPrimitiveElement(collection, i))
			{
				continue;
			}
			
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

		if (!elementType.IsSubclassOf(typeof(LinkConfig)))
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
		if (GUILayout.Button("ⓧ", GUILayout.Width(20)))
		{
			RemoveElement(collection, index);
		}

		if (index < collection.Count - 1)
		{
			if (GUILayout.Button("↓", GUILayout.Width(30)))
			{
				UpMoveElement(collection, index);
			}
		}

		if (index != 0)
		{
			if (GUILayout.Button("↑", GUILayout.Width(30)))
			{
				DownMoveElement(collection, index);
			}
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
		var item = GetByIndex(collection, index);
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
		var filterCollection = new List<object>();
		
		foreach (var obj in collection)
		{
			if (CheckFilter(collection, collection.IndexOf(obj)))
			{
				filterCollection.Add(obj);
			}
		}
		
		OnGuiCollectionObjectItem(filterCollection, index);
	}

	private bool CheckFilter(IList collection,
		int index)
	{
		var item = GetByIndex(collection, index);

		if (!TryGetPrimaryKey(item, out var primaryKey))
		{
			return true;
		}
		
		var filter = _filter.ToLower();
		var result = primaryKey.ToLower().Contains(filter);

		return result;
	}

	private bool TryGuiCollectionPrimitiveElement(IList collection,
		int index)
	{
		var item = GetByIndex(collection, index);

		if (item == null)
		{
			return false;
		}
		
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
		var item = GetByIndex(collection, index);
		var title = GetGroupTitle(index, item);

		using (new EditorVerticalGroup(17, "GroupBox"))
		{
			OnGuiCollectionObjectElementTopPanel(collection, index);
			FillExtraFieldElements(index + 1);

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
		var result = $"Index : {index}";
			
		if (TryGetPrimaryKey(item, out var primaryKey))
		{
			result = $"Index : {index} | PrimaryKey : \"{primaryKey}\"";
		}

		return result;
	}

	private static bool TryGetPrimaryKey(object item, out string primaryKey)
	{
		primaryKey = "##";

		if (item == null)
		{
			return false;
		}
		
		var fields = item.GetType().GetMembers();

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

			return true;
		}

		return false;
	}

	private static void OnGuiCollectionObjectElementTopPanel(IList collection,
		int index)
	{
		using (new EditorHorizontalGroup(17))
		{
			if (GUILayout.Button("remove ⓧ", GUILayout.MaxWidth(80)))
			{
				RemoveElement(collection, index);
			}
			
			GUILayout.Space(20);
			
			if (index < collection.Count - 1)
			{
				if (GUILayout.Button("↓", GUILayout.MaxWidth(30)))
				{
					UpMoveElement(collection, index);
				}
			}

			if (index != 0)
			{
				if (GUILayout.Button("↑", GUILayout.MaxWidth(30)))
				{
					DownMoveElement(collection, index);
				}
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
				OnGuiCollectionMaxPanel(collection);
			}

			var item = GetByIndex(collection, 0);

			if (TryGetPrimaryKey(item, out _))
			{
				EditorGUILayout.Space();
				EditorLayoutUtility.ToolbarSearch(ref _filter, 17);
			}
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

	private void FillSupportedTypes(Type fieldType)
	{
		if (_supportedTypes != null)
		{
			return;
		}

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

		_addTypeIndex = options.Length > 1 
			? EditorGUILayout.Popup(_addTypeIndex, options) 
			: 0;
	}

	private void OnGuiCollectionPagesPanel(IList collection)
	{
		if (collection.Count <= 0)
		{
			return;
		}

		var ceilingCountPages = SetCollectionPageCounter(collection);

		if (ceilingCountPages <= 1)
		{
			return;
		}

		GUI.enabled = _currentPage > 1;

		if (GUILayout.Button("<"))
		{
			_currentPage--;
			EditorGUI.FocusTextInControl(null);
		}

		GUI.enabled = true;

		_currentPage = EditorGUILayout.IntField(_currentPage, GUILayout.MaxWidth(50));
		EditorGUILayout.LabelField($"/  {ceilingCountPages}", GUILayout.MaxWidth(60));

		GUI.enabled = _currentPage < ceilingCountPages;

		if (GUILayout.Button(">"))
		{
			_currentPage++;
			EditorGUI.FocusTextInControl(null);
		}

		GUI.enabled = true;
	}

	private int SetCollectionPageCounter(IList collection)
	{
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

		return ceilingCountPages;
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

	private void OnGuiCollectionMaxPanel(IList collection)
	{
		if (collection.Count <= 1)
		{
			_isMaxFlag = true;
			_maxShow = 1;

			return;
		}

		var maxShowTemp = _maxShow;
		_maxShow = EditorGUILayout.IntField(_maxShow, GUILayout.MaxWidth(50));

		if (_isMaxFlag && maxShowTemp > _maxShow)
		{
			_isMaxFlag = false;
			EditorGUI.FocusTextInControl(null);
		}

		var isMaxFlagTemp = _isMaxFlag;
		_isMaxFlag = GUILayout.Toggle(_isMaxFlag, "Max", GUI.skin.button);

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
		if (elementType == typeof(long))
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

	private static object GetByIndex(IList collection,
		int index)
	{
		if (collection.Count == 0)
		{
			return null;
		}

		if (index < 0 || index >= collection.Count)
		{
			return null;
		}
		
		var obj = collection[index];

		if (obj == null)
		{
			collection.RemoveAt(index);
		}

		return obj;
	}

	#endregion
}

}