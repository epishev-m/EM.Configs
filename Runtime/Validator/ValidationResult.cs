using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Configs
{

public sealed class ValidationResult
{
	private readonly string _validationType;

	private readonly List<ValueTuple<string, string>> _errors = new();

	#region ValidationResult

	public ValidationResult(string validationType)
	{
		_validationType = validationType;
	}

	public string ValidationType => _validationType;

	public IReadOnlyList<ValueTuple<string, string>> Errors => _errors;

	public void AddResult(string path,
		string errorMessage)
	{
		_errors.Add(new ValueTuple<string, string>(path, errorMessage));
	}

	#endregion

	#region Object

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{_validationType}:");

		foreach (var valueTuple in _errors)
		{
			stringBuilder.AppendLine($" - {valueTuple.Item2} Path: {valueTuple.Item1}");
		}

		return stringBuilder.ToString();
	}

	#endregion
}

}