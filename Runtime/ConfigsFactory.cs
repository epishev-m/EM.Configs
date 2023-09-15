using System.IO;
using EM.Foundation;
using UnityEngine;

namespace EM.Configs
{

public abstract class ConfigsFactory<T> : IFactory
	where T : class, new()
{
	private readonly IAssetsManager _assetsManager;

	private readonly ILibraryEntryCatalog _catalog;

	private readonly IConfigsSerializerFactory _factory;

	#region IFactory

	public Result<object> Create()
	{
		var loadAssetResult = _assetsManager.LoadAsset<TextAsset>(Key);

		if (loadAssetResult.Failure)
		{
			return new ErrorResult<object>(ConfigsFactoryStringResources.FailedToLoad(this));
		}

		var textAsset = loadAssetResult.Data;
		var serializer = _factory.Create(_catalog);
		var stream = new MemoryStream(textAsset.bytes);
		var result = serializer.Deserialize<T>(stream);
		_assetsManager.ReleaseAsset(textAsset);

		if (result.Failure)
		{
			return new ErrorResult<object>(ConfigsFactoryStringResources.ErrorDeserialization(this));
		}

		return new SuccessResult<object>(result.Data);
	}

	#endregion

	#region ConfigsFactory

	protected ConfigsFactory(IAssetsManager assetsManager,
		ILibraryEntryCatalog catalog,
		IConfigsSerializerFactory factory)
	{
		Requires.NotNullParam(assetsManager, nameof(assetsManager));

		_assetsManager = assetsManager;
		_catalog = catalog;
		_factory = factory;
	}

	protected abstract string Key
	{
		get;
	}

	#endregion
}

}