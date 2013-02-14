using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DelegateQueryable;
namespace DelegateQueryable.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CanSkipAndTake()
        {
            DataQuery<string> getPageFromDataSource = (info) => SomeDataSource(info.Skip, info.Take);

            IQueryable<string> pq = new DelegateQueryable<string>(getPageFromDataSource);
            
            var items = pq.Skip(3).Take(2);

            Assert.AreEqual("Item 4,Item 5", string.Join(",", items));

        }

        static IEnumerable<string> SomeDataSource(int startIndex, int pageSize)
        {
            var items = Enumerable.Range(1, 1001).Select(i => "Item " + i);
            return items.Skip(startIndex).Take(pageSize);
        }
    }
    
}
