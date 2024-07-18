namespace HyperVectorDB.Tests;

[TestFixture]
public class ProcessorTests
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
    public void PreprocessorTest()
    {
        HyperVectorDB DB = new HyperVectorDB(new Embedder.LmStudio(), "TestDatabase");
        DB.IndexDocument("This is a test document about dogs", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about cats", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about fish", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about birds", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about dogs and cats", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about cats and fish", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about fish and birds", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about birds and dogs", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about dogs and cats and fish", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about cats and fish and birds", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about fish and birds and dogs", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about birds and dogs and cats", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about dogs and cats and fish and birds", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about cats and fish and birds and dogs", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about fish and birds and dogs and cats", TestPreprocessor, null, "TestDatabase");
        DB.IndexDocument("This is a test document about birds and dogs and cats and fish", TestPreprocessor, null, "TestDatabase");
        DB.Save();
        var result = DB.QueryCosineSimilarity("dogs", 1);
        var document = result.Documents[0];
        Assert.IsNotNull(document);
        Assert.IsTrue(document.DocumentString == document.DocumentString.ToUpperInvariant());
        Assert.Pass();
    }

    [Test]
    public void PostprocessorTest()
    {
        HyperVectorDB DB = new HyperVectorDB(new Embedder.LmStudio(), "TestDatabase");
        DB.IndexDocument("This is a test document about dogs", null, TestPostprocessor, "TestDatabase");
        DB.Save();
        var result = DB.QueryCosineSimilarity("dogs", 1);
        var document = result.Documents[0];
        Assert.IsNotNull(document);
        Assert.IsTrue(document.DocumentString == document.DocumentString.ToLowerInvariant());
        Assert.Pass();
    }

    private string? TestPreprocessor(string line, string? path, int? lineNumber)
    {
        return line.ToUpperInvariant();
    }

    private string? TestPostprocessor(string line, string? path, int? lineNumber)
    {
        return line.ToLowerInvariant();
    }



}