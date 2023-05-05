namespace EM.Configs.Editor
{

using System;
using System.Reflection;
using Assistant.Editor;
using UnityEditor;
using UnityEditor.AnimatedValues;

public sealed class DefinitionsAssistantObject
{
	private readonly Action<object> _guiFields;

	private readonly AnimBool _showExtraFields = new(false);

	#region DefinitionsObject

	public DefinitionsAssistantObject(EditorWindow window,
		Action<object> guiFields)
	{
		_guiFields = guiFields;
		_showExtraFields.valueChanged.AddListener(window.Repaint);
	}

	public void DoLayoutObject(FieldInfo field,
		object fieldValue)
	{
		using var fadeGroup = new EditorFadeGroup(field.Name, _showExtraFields);

		if (!fadeGroup.IsVisible)
		{
			return;
		}

		using (new EditorVerticalGroup(17))
		{
			_guiFields.Invoke(fieldValue);
		}
	}

	#endregion
}

}