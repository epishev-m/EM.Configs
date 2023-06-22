namespace EM.Configs
{

using Foundation;
using MessagePack;
using UnityEngine;

public class ConfigsFactory<T> : IFactory
	where T : class
{
	private readonly IAssetsManager _assetsManager;

	private readonly string _key;

	#region IFactory

	public Result<object> Create()
	{
		var loadAssetResult = _assetsManager.LoadAsset<TextAsset>(_key);

		if (loadAssetResult.Failure)
		{
			return new ErrorResult<object>(ConfigsFactoryStringResources.FailedToLoad(this));
		}

		var textAsset = loadAssetResult.Data;
		Result<object> result;

		try
		{
			var instance = MessagePackSerializer.Deserialize<T>(textAsset.bytes);
			result = new SuccessResult<object>(instance);
		}
		catch (MessagePackSerializationException)
		{
			result = new ErrorResult<object>(ConfigsFactoryStringResources.ErrorDeserialization(this));
		}
		finally
		{
			_assetsManager.ReleaseAsset(textAsset);
		}

		return result;
	}

	#endregion

	#region ConfigsFactory

	public ConfigsFactory(IAssetsManager assetsManager,
		string key)
	{
		Requires.NotNullParam(assetsManager, nameof(assetsManager));

		_assetsManager = assetsManager;
		_key = key;
	}

	#endregion
}

}