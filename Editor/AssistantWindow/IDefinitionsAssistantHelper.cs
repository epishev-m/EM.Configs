namespace EM.Configs.Editor
{

using System.Collections.Generic;

public interface IDefinitionsAssistantHelper
{
	IEnumerable<string> GetIds(ConfigLink configLink,
		object config);
}

}