using Scriba.JsonFactory;

namespace Scriba.JsonFactory.Test;

public class PoolTests
{
    private class PoolTestObj
    {
        public int Id { get; set; }
    }

    [Test]
    public void New_ReturnsNonNullObject()
    {
        var obj = Pool<PoolTestObj>.New();
        Assert.That(obj, Is.Not.Null);
    }

    [Test]
    public void FreeThenNew_ReusesObject()
    {
        var obj1 = Pool<PoolTestObj>.New();
        Pool<PoolTestObj>.Free(obj1);
        var obj2 = Pool<PoolTestObj>.New();
        Assert.That(obj2, Is.SameAs(obj1));
    }

    [Test]
    public void New_ReturnsDistinctInstances_WhenNotFreed()
    {
        var obj1 = Pool<PoolTestObj>.New();
        var obj2 = Pool<PoolTestObj>.New();
        Assert.That(obj2, Is.Not.SameAs(obj1));
    }

    [Test]
    public void FreeAndNew_MultipleObjects_RecyclesAll()
    {
        const int count = 20;
        var items = new PoolTestObj[count];

        for (int i = 0; i < count; i++)
            items[i] = Pool<PoolTestObj>.New();

        foreach (var item in items)
            Pool<PoolTestObj>.Free(item);

        var recycled = new List<PoolTestObj>();
        for (int i = 0; i < count; i++)
            recycled.Add(Pool<PoolTestObj>.New());

        Assert.That(recycled, Is.EquivalentTo(items));
    }

    [Test]
    public void ConcurrentAccess_DoesNotThrow()
    {
        var tasks = new Task[Environment.ProcessorCount * 2];
        for (int t = 0; t < tasks.Length; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var obj = Pool<PoolTestObj>.New();
                    Pool<PoolTestObj>.Free(obj);
                }
            });
        }
        Assert.That(() => Task.WaitAll(tasks), Throws.Nothing);
    }

    [Test]
    public void DifferentTypePools_AreIndependent()
    {
        var objA = Pool<PoolTestObj>.New();
        var objB = Pool<object>.New();

        Pool<PoolTestObj>.Free(objA);
        Pool<object>.Free(objB);

        var recycledA = Pool<PoolTestObj>.New();
        var recycledB = Pool<object>.New();

        Assert.That(recycledA, Is.SameAs(objA));
        Assert.That(recycledB, Is.SameAs(objB));
    }
}
