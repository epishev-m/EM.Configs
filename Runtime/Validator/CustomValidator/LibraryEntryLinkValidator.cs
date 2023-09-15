using System.Reflection;

namespace EM.Configs
{

public sealed class LibraryEntryLinkValidator : ICustomValidator
{
	private readonly ILibraryEntryCatalog _catalog;

	#region ICustomValidator

	public string ValidationType => nameof(LibraryEntryLinkValidator);

	public bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value == null)
		{
			return true;
		}

		if (value is not BaseLibraryEntryLink link)
		{
			return true;
		}

		var entryType = link.GetEntryType();
		var libraryEntry = _catalog.Find(entryType, link.Id);

		if (libraryEntry == null)
		{
			errorMessage = $"An object with type \"{entryType}\" and identifier \"{link.Id}\" was not found." +
			               $" LibraryEntryLink cannot refer to a non-existent object.";
		}

		return string.IsNullOrWhiteSpace(errorMessage);
	}

	#endregion

	#region LibraryEntryLinkCustomValidator

	public LibraryEntryLinkValidator(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}