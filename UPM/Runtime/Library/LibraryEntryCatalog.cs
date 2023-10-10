using System;
using System.Collections.Generic;
using System.Linq;
using EM.Foundation;

namespace EM.Configs
{

public sealed class LibraryEntryCatalog : ILibraryEntryCatalog
{
	private readonly List<ILibraryEntry> _entities = new();

	private Dictionary<Type, List<ILibraryEntry>> _lookupByType;

	private Dictionary<Type, Dictionary<string, ILibraryEntry>> _lookupByTypeAndName;

	#region ILibraryEntryCatalog

	public void Initialize()
	{
		Requires.ValidOperation(_lookupByType == null, nameof(Register));
		Requires.ValidOperation(_lookupByTypeAndName == null, nameof(Register));

		var lookup = _entities.ToLookup(x => x.GetType());
		_lookupByType = lookup.ToDictionary(x => x.Key, y => y.ToList());
		_lookupByTypeAndName = lookup.ToDictionary(x => x.Key, y => y.ToDictionary(x => x.Id));
	}

	public void Register(ILibraryEntry entry)
	{
		Requires.NotNullParam(entry, nameof(entry));
		Requires.ValidOperation(_lookupByType == null, nameof(Register));
		Requires.ValidOperation(_lookupByTypeAndName == null, nameof(Register));

		_entities.Add(entry);
	}

	public IEnumerable<ILibraryEntry> All()
	{
		Requires.ValidOperation(_lookupByType != null, nameof(Register));
		Requires.ValidOperation(_lookupByTypeAndName != null, nameof(Register));

		return _entities;
	}

	public IEnumerable<ILibraryEntry> All(Type type)
	{
		Requires.ValidOperation(_lookupByType != null, nameof(Register));
		Requires.ValidOperation(_lookupByTypeAndName != null, nameof(Register));

		if (_lookupByType.TryGetValue(type, out var resultList))
		{
			return resultList;
		}

		return Enumerable.Empty<ILibraryEntry>(); 
	}

	public ILibraryEntry Find(Type type,
		string name)
	{
		Requires.ValidOperation(_lookupByType != null, nameof(Register));
		Requires.ValidOperation(_lookupByTypeAndName != null, nameof(Register));

		if (!_lookupByTypeAndName.TryGetValue(type, out var lookupByName))
		{
			return null;
		}

		if (lookupByName.TryGetValue(name, out var entry))
		{
			return entry;
		}

		return null;
	}

	#endregion
}

}