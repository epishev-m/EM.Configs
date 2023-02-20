﻿namespace EM.Configs
{

using System;
using Foundation;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class,
	AllowMultiple = true,
	Inherited = false)]
public sealed class UnionAttribute : Attribute
{
	public Type Type { get; }

	public UnionAttribute(Type type)
	{
		Requires.NotNull(type, nameof(type));

		Type = type;
	}
}

}