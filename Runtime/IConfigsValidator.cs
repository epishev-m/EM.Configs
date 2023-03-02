namespace EM.Configs
{

public interface IConfigsValidator
{
	string ErrorMassage { get; }

	bool Validate(object config);
}

}