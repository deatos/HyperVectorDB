using NUnit.Framework.Internal;

namespace HyperVectorDB.Tests;

[TestFixture]
public class HVDBDocumentTests
{
    [Test]
    public void ConstructorValidation()
    {
        HVDBDocument a = new();
        HVDBDocument b = new HVDBDocument();
        HVDBDocument c = new HVDBDocument("test");
        HVDBDocument d = new HVDBDocument("test_id", "test");

        Assert.Multiple(() =>
        {
            Assert.That(a.DocumentString == string.Empty, Is.True);
            Assert.That(b.DocumentString == string.Empty, Is.True);
            Assert.That(c.DocumentString == "test", Is.True);
            Assert.That(d.DocumentString == "test", Is.True);

            Assert.That(a.ID, Is.Not.EqualTo(b.ID));
            Assert.That(a.ID, Is.Not.EqualTo(c.ID));
            Assert.That(b.ID, Is.Not.EqualTo(c.ID));
            Assert.That(d.ID, Is.EqualTo("test_id"));
        });

        Assert.Pass();
    }

}