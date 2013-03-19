using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using LinqToAnything;
namespace LinqToAnything.Tests
{
    [TestFixture]
    public class Tests
    {
        private static int Skipped;
        private static int? Taken;

        [Test]
        public void CanSkipAndTake()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);

            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);
            
            var items = pq.Skip(3).Take(2).ToArray();

            Assert.AreEqual(3, Skipped);
            Assert.AreEqual(2, Taken);

            Assert.AreEqual("Item 4,Item 5", string.Join(",", items));

        }

        [Test]
        public void CanWorkWithoutQuery()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);

            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);

            var items = pq.ToArray();

            Assert.AreEqual(0, Skipped);
            Assert.AreEqual(null, Taken);

            Assert.AreEqual("Item 1,Item 2", string.Join(",", items.Take(2)));

        }

        [Test]
        public void CanHandleAProjection()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);
            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);
            var items = pq.Select(s => new Projection(){ Item = s });
            Assert.AreEqual("Item 1", items.ToArray().First().Item);
        }

        [Test]
        public void CanHandleASkipATakeAndAProjection()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);
            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 2", items.ToArray().First().Item);
        }
        [Test]
        public void CanHandleAProjectionASkipAndATake()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);
            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 2", items.ToArray().First().Item);
        }


        
        static IEnumerable<string> SomeDataSource(int skip, int? take)
        {
            Skipped = skip;
            Taken = take;
            var items = Enumerable.Range(1, 1001).Select(i => "Item " + i);
            items = items.Skip(skip);
            if (take != null) items = items.Take(take.Value);
            return items;
        }
    }

    public class Projection
    {
        public string Item { get; set; }
    }
}
