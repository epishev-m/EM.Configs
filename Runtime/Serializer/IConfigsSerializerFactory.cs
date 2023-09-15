namespace EM.Configs
{

public interface IConfigsSerializerFactory
{
	IConfigsSerializer Create(ILibraryEntryCatalog catalog);
}

}