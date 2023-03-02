using System;

namespace EM.Configs
{

public sealed class ValidateWithAttribute : Attribute
{
	public readonly Type Type;

	public ValidateWithAttribute(Type type)
	{
		Type = type;
	}
}

}