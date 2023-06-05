using System;
using EM.Configs;
using NUnit.Framework;

internal sealed class LinkDefinitionTests
{
	[Test]
	public void LinkDefinition_Constructor_ExceptionType()
	{
		// Arrange
		var actual = false;

		// Act
		try
		{
			var unused = new LinkDefinitionTest(null);
		}
		catch (ArgumentNullException)
		{
			actual = true;
		}

		//Assert
		Assert.IsTrue(actual);
	}

	[Test]
	public void LinkDefinitionT_Constructor_TypeAndName()
	{
		const string expectedName = "test";

		// Act
		var linkDefinition = new LinkDefinition<Test>
		{
			Id = expectedName
		};
		var actualType = linkDefinition.Type;
		var actualName = linkDefinition.Id;

		// Assert
		Assert.AreEqual(typeof(Test), actualType);
		Assert.AreEqual(expectedName, actualName);
	}

	#region Nested

	private sealed class LinkDefinitionTest : LinkDefinition
	{
		public LinkDefinitionTest(Type entryType)
			: base(entryType)
		{
		}
	}
	
	private sealed class Test
	{
	}

	#endregion
}
