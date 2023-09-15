using System.Linq;
using System.Reflection;

namespace EM.Configs
{

public sealed class LibraryEntryValidator : ICustomValidator
{
	private readonly ILibraryEntryCatalog _catalog;

	#region ICustomValidator

	public string ValidationType => nameof(LibraryEntryValidator);

	public bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		if (value == null)
		{
			return true;
		}

		if (value is not ILibraryEntry libraryEntry)
		{
			return true;
		}

		var type = value.GetType();
		var count = _catalog.All(type).Count(entry => entry.Id == libraryEntry.Id);

		if (count > 1)
		{
			errorMessage = $"The catalog should not contain several entities of the same type \"{type}\" " +
			               "and at the same time with the same identifier \"{libraryEntry.Id}\"";
		}

		return string.IsNullOrWhiteSpace(errorMessage);
	}

	#endregion

	#region LibraryEntryValidator

	public LibraryEntryValidator(ILibraryEntryCatalog catalog)
	{
		_catalog = catalog;
	}

	#endregion
}

}