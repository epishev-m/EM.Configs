using System;
using EM.Configs;
using EM.Foundation;
using NUnit.Framework;

public sealed class BaseConfigsFactoryTest
{
	[Test]
	public void BaseConfigsFactory_Constructor_Exception()
	{
		// Arrange
		var actual = false;

		// Act
		try
		{
			var unused = new ConfigsFactory<TestConfig>(null, string.Empty);
		}
		catch (ArgumentNullException)
		{
			actual = true;
		}

		// Assert
		Assert.IsTrue(actual);
	}

	#region Nested

	private sealed class TestConfig
	{
	}

	#endregion
}