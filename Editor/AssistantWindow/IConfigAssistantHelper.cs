namespace EM.Configs.Editor
{

using System.Collections.Generic;

public interface IConfigAssistantHelper
{
	IEnumerable<string> GetIds(LinkConfig linkConfig);
}

}