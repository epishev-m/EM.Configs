namespace EM.Configs.Editor
{

using System.Collections.Generic;

public interface IDefinitionAssistantHelper
{
	IEnumerable<string> GetIds(DefinitionLink definitionLink,
		object config);
}

}