namespace EM.Configs.Editor
{

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

public sealed class ConfigAssistantLink
{
	private readonly IConfigAssistantHelper _externalHelper;

	#region DefinitionsAssistantLink

	public ConfigAssistantLink(IConfigAssistantHelper externalHelper)
	{
		_externalHelper = externalHelper;
	}

	public void DoLayoutLink(MemberInfo field,
		object fieldValue)
	{
		if (fieldValue is not LinkConfig link)
		{
			return;
		}

		var options = GetOptions(link);
		var index = options.IndexOf(link.Id);

		if (index == -1)
		{
			index = 0;
		}

		index = EditorGUILayout.Popup(field.Name, index, options.ToArray());
		link.Id = options[index];
	}

	private List<string> GetOptions(LinkConfig link)
	{
		var options = _externalHelper.GetIds(link).ToList();

		if (!string.IsNullOrWhiteSpace(link.Id))
		{
			if (options.Any(o => o != link.Id))
			{
				options.Add(link.Id);
			}
		}
		else
		{
			link.Id = string.Empty;
		}

		if (!options.Any())
		{
			options.Add("none");
		}

		return options;
	}

	#endregion
}

}