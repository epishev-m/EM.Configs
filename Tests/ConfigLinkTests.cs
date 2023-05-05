using System;
using EM.Configs;
using NUnit.Framework;

internal sealed class ConfigLinkTests
{
	[Test]
	public void ConfigLink_Constructor_ExceptionType()
	{
		// Arrange
		var actual = false;

		// Act
		try
		{
			var unused = new ConfigLinkTest(null);
		}
		catch (ArgumentNullException)
		{
			actual = true;
		}

		//Assert
		Assert.IsTrue(actual);
	}

	[Test]
	public void ConfigLinkT_Constructor_TypeAndName()
	{
		const string expectedName = "test";

		// Act
		var configLink = new ConfigLink<Test>
		{
			Id = expectedName
		};
		var actualType = configLink.Type;
		var actualName = configLink.Id;

		// Assert
		Assert.AreEqual(typeof(Test), actualType);
		Assert.AreEqual(expectedName, actualName);
	}

	#region Nested

	private sealed class ConfigLinkTest : ConfigLink
	{
		public ConfigLinkTest(Type entryType)
			: base(entryType)
		{
		}
	}
	
	private sealed class Test
	{
	}

	#endregion
}
