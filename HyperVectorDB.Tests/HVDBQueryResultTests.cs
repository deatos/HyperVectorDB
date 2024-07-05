namespace HyperVectorDB.Tests;

[TestFixture]
public class HVDBQueryResultTests
{

    [Test]
    public void ConstructorValidation()
    {
        Random random = new Random(0);
        List<HVDBDocument> documents = new();
        List<double> distances = new();

        for(int i = 0; i < 100_000; i++)
        {
            documents.Add(new HVDBDocument(random.Next().ToString()));
            distances.Add(random.NextDouble());
        }

        HVDBQueryResult a = new(documents, distances);

        Assert.Pass();
    }

}