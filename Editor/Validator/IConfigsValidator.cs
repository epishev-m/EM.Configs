namespace EM.Configs.Editor
{

public interface IConfigsValidator
{
	string ErrorMassage { get; }

	bool Validate(object config);
}

}