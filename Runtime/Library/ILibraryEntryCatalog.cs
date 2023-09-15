using System;
using System.Collections.Generic;

namespace EM.Configs
{

public interface ILibraryEntryCatalog
{
	void Initialize();

	void Register(ILibraryEntry entry);

	IEnumerable<ILibraryEntry> All();

	IEnumerable<ILibraryEntry> All(Type type);

	IEnumerable<ILibraryEntry> All<T>()
		where T : ILibraryEntry
	{
		return All(typeof(T));
	}

	ILibraryEntry Find(Type type,
		string name);

	ILibraryEntry Find<T>(string name)
		where T : ILibraryEntry
	{
		return Find(typeof(T), name);
	}
}

}