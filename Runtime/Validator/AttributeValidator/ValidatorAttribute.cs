using System;
using System.Reflection;

namespace EM.Configs
{

[AttributeUsage(AttributeTargets.Field)]
public abstract class ValidatorAttribute : Attribute
{
	public abstract string ValidationType { get; }

	public abstract bool TryValidate(FieldInfo fieldInfo,
		object value,
		out string errorMessage);
}

}