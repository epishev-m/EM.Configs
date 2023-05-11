using System;
using EM.Configs;
using NUnit.Framework;

internal sealed class DefinitionLinkTests
{
	[Test]
	public void DefinitionLink_Constructor_ExceptionType()
	{
		// Arrange
		var actual = false;

		// Act
		try
		{
			var unused = new DefinitionLinkTest(null);
		}
		catch (ArgumentNullException)
		{
			actual = true;
		}

		//Assert
		Assert.IsTrue(actual);
	}

	[Test]
	public void DefinitionLinkT_Constructor_TypeAndName()
	{
		const string expectedName = "test";

		// Act
		var definitionLink = new DefinitionLink<Test>
		{
			Id = expectedName
		};
		var actualType = definitionLink.Type;
		var actualName = definitionLink.Id;

		// Assert
		Assert.AreEqual(typeof(Test), actualType);
		Assert.AreEqual(expectedName, actualName);
	}

	#region Nested

	private sealed class DefinitionLinkTest : DefinitionLink
	{
		public DefinitionLinkTest(Type entryType)
			: base(entryType)
		{
		}
	}
	
	private sealed class Test
	{
	}

	#endregion
}
