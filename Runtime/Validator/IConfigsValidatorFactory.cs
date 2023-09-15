namespace EM.Configs
{

public interface IConfigsValidatorFactory
{
	IValidator Create(ILibraryEntryCatalog catalog);
}

}