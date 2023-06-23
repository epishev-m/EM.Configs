namespace EM.Configs.Editor
{

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EM.Assistant.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.U2D;

public sealed class ConfigAssistantSpriteAtlas
{
	private readonly AnimBool _showExtraFields = new(false);

	#region DefinitionAssistantSpriteAtlas
	
	public ConfigAssistantSpriteAtlas(EditorWindow window)
	{
		_showExtraFields.valueChanged.AddListener(window.Repaint);
	}

	public void DoLayoutSpriteAtlas(FieldInfo field,
		object fieldValue,
		bool useGroup)
	{
		if (fieldValue is not ISpriteAtlas value)
		{
			return;
		}

		if (!useGroup)
		{
			using var fadeGroup = new EditorFadeGroup(field.Name, _showExtraFields);

			if (fadeGroup.IsVisible)
			{
				OnGuiSpriteAtlas(value);
			}
		}
		else
		{
			OnGuiSpriteAtlas(value);
		}
	}

	private static bool TryGetSpriteAtlas(ISpriteAtlas fieldValue,
		out SpriteAtlas spriteAtlas)
	{
		spriteAtlas = default;

		if (!string.IsNullOrWhiteSpace(fieldValue.Atlas))
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			var allEntries = settings.groups.SelectMany(g => g.entries).ToList();
			var foundEntry = allEntries.FirstOrDefault(e => e.address == fieldValue.Atlas);

			if (foundEntry != null)
			{
				spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(foundEntry.AssetPath);
			}
		}

		spriteAtlas = (SpriteAtlas) EditorGUILayout.ObjectField("Atlas", spriteAtlas, typeof(SpriteAtlas), false);
		fieldValue.Atlas = spriteAtlas != null ? spriteAtlas.name : string.Empty;

		return spriteAtlas != default;
	}

	private static bool TryGetOptions(SpriteAtlas spriteAtlas,
		out IEnumerable<string> options)
	{
		options = new List<string>();
		var sprites = new Sprite[spriteAtlas.spriteCount];
		spriteAtlas.GetSprites(sprites);
		const string cloneSubString = "(Clone)";
		options = sprites.Select(sprite => sprite.name.TrimEnd(cloneSubString.ToCharArray()));

		return options.Any();
	}

	private static void OnGuiSpriteAtlas(ISpriteAtlas fieldValue)
	{
		using (new EditorVerticalGroup(17))
		{
			if (TryGetSpriteAtlas(fieldValue, out var spriteAtlas))
			{
				if (TryGetOptions(spriteAtlas, out var options))
				{
					OnGuiSprite(options, fieldValue);
				}
			}
		}
	}

	private static void OnGuiSprite(IEnumerable<string> options,
		ISpriteAtlas fieldValue)
	{
		var optionsList = options.ToList();
		var index = optionsList.IndexOf(fieldValue.Sprite);

		if (index == -1)
		{
			optionsList.Add(fieldValue.Sprite);
			index = optionsList.IndexOf(fieldValue.Sprite);
		}

		index = EditorGUILayout.Popup("Sprite", index, optionsList.ToArray());

		fieldValue.Sprite = optionsList[index];
	}
	
	#endregion
}

}