using System;

namespace EM.Configs
{

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class OptionalFieldAttribute : Attribute
{
}

}