using System.Linq;
using NUnit.Framework;

namespace DelegateQueryable.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {
            var items = Enumerable.Range(1, 1001);
            var getPage = new DataQuery<int>((info) => items.Skip(info.Skip).Take(info.Take));

            var pq = new DelegateQueryable<int>(getPage);
            
            Assert.AreEqual("1,2,3,4,5,6,7,8,9,10", string.Join(",", pq.Take(10).Skip(0)));
            
            var queryable = pq.Skip(3).Take(10);

            Assert.AreEqual("4,5,6,7,8,9,10,11,12,13", string.Join(",", queryable));
        }
    }
    
}
