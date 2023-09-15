using System.Collections.Generic;

namespace EM.Configs
{

public interface IValidator
{
	bool TryValidate(object instance,
		List<ValidationResult> validationResults);
}

}