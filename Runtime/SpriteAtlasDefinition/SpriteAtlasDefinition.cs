namespace EM.Configs
{

using MessagePack;
using System;

[Serializable]
[MessagePackObject]
public sealed class SpriteAtlasDefinition
{
	public string Atlas;
	
	public string Sprite;
}

}