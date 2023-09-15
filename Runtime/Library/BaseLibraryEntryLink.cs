using System;
using EM.Foundation;

namespace EM.Configs
{

[Serializable]
public abstract class BaseLibraryEntryLink
{
	protected readonly Type EntryType;

	protected ILibraryEntryCatalog Catalog;

	public abstract string Id { get; set; }

	protected BaseLibraryEntryLink(Type entryType)
	{
		Requires.NotNullParam(entryType, nameof(entryType));

		EntryType = entryType;
	}

	public void SetCatalog(ILibraryEntryCatalog catalog)
	{
		Requires.NotNullParam(catalog, nameof(catalog));

		Catalog = catalog;
	}

	public Type GetEntryType()
	{
		return EntryType;
	}
}

[Serializable]
public abstract class BaseLibraryEntryLink<T> : BaseLibraryEntryLink
	where T : class
{
	protected T Value;

	#region BaseLibraryEntryLink

	protected BaseLibraryEntryLink()
		: base(typeof(T))
	{
	}

	public T Unwrap()
	{
		return Value ??= Catalog.Find(EntryType, Id) as T;
	}

	public static implicit operator T(BaseLibraryEntryLink<T> link)
	{
		return link?.Unwrap();
	}

	#endregion
}

}