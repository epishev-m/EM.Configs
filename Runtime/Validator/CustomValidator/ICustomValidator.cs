using System.Reflection;

namespace EM.Configs
{

public interface ICustomValidator
{
	string ValidationType { get; }

	bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage);
}

}