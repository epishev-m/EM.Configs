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
			var unused = new LinkConfigTest(null);
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
		var linkDefinition = new LinkConfigTest<Test>
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
	
	private sealed class LinkConfigTest<T> : LinkConfig<T>
		where T : class
	{
		public override string Id { get; set; }
	}

	private sealed class LinkConfigTest : LinkConfig
	{
		public LinkConfigTest(Type entryType)
			: base(entryType)
		{
		}

		public override string Id { get; set; }
	}
	
	private sealed class Test
	{
	}

	#endregion
}
