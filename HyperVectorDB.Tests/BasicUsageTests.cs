using NUnit.Framework.Legacy;
using NUnit.Framework;

namespace HyperVectorDB.Tests;

[TestFixture]
public class BasicUsageTests
{
    [SetUp]
    public void Setup()
    {
        if (Directory.Exists("TestDatabase"))
        {
            Directory.Delete("TestDatabase", true);
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (Directory.Exists("TestDatabase"))
        {
            Directory.Delete("TestDatabase", true);
        }
    }

    [Test]
    public void BasicUsage()
    {
        HyperVectorDB DB = new HyperVectorDB(new Embedder.LmStudio(), "TestDatabase");
        DB.CreateIndex("TestIndex");

        DB.IndexDocument("TestIndex", "This is a test document about dogs");
        DB.IndexDocument("TestIndex", "This is a test document about cats");
        DB.IndexDocument("TestIndex", "This is a test document about fish");
        DB.IndexDocument("TestIndex", "This is a test document about birds");
        DB.IndexDocument("TestIndex", "This is a test document about dogs and cats");
        DB.IndexDocument("TestIndex", "This is a test document about cats and fish");
        DB.IndexDocument("TestIndex", "This is a test document about fish and birds");
        DB.IndexDocument("TestIndex", "This is a test document about birds and dogs");
        DB.IndexDocument("TestIndex", "This is a test document about dogs and cats and fish");
        DB.IndexDocument("TestIndex", "This is a test document about cats and fish and birds");
        DB.IndexDocument("TestIndex", "This is a test document about fish and birds and dogs");
        DB.IndexDocument("TestIndex", "This is a test document about birds and dogs and cats");
        DB.IndexDocument("TestIndex", "This is a test document about dogs and cats and fish and birds");
        DB.IndexDocument("TestIndex", "This is a test document about cats and fish and birds and dogs");
        DB.IndexDocument("TestIndex", "This is a test document about fish and birds and dogs and cats");
        DB.IndexDocument("TestIndex", "This is a test document about birds and dogs and cats and fish");

        DB.Save();
        DB = new HyperVectorDB(new Embedder.LmStudio(), "TestDatabase");
        DB.Load();

        var result = DB.QueryCosineSimilarity("dogs");
        ClassicAssert.IsTrue(result.Documents.Count == 5);
        result = DB.QueryCosineSimilarity("cats", 10);
		ClassicAssert.IsTrue(result.Documents.Count == 10);
        result = DB.QueryCosineSimilarity("fish", 3);
		ClassicAssert.IsTrue(result.Documents.Count == 3);
        result = DB.QueryCosineSimilarity("birds", 1);
		ClassicAssert.IsTrue(result.Documents.Count == 1);

		ClassicAssert.Pass();
    }
}