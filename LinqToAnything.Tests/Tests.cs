using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using LinqToAnything;
using System.Linq.Dynamic.Core;
namespace LinqToAnything.Tests
{
    [TestFixture]
    public class Tests
    {
        private static int Skipped;
        private static int? Taken;
        private static IEnumerable<SomeEntity> Data = Enumerable.Range(1, 10).Select(i => new SomeEntity { Index = i, Name = "Item " + i.ToString().PadLeft(2, '0') });

        [Test]
        public void CanSkipAndTake()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            
            //When I skip and then take
            var items = pq.Skip(3).Take(2).ToArray();

            //Then the correct items should be returned
            Assert.AreEqual(3, Skipped);
            Assert.AreEqual(2, Taken);
            Assert.AreEqual("Item 04,Item 05", string.Join(",", items.Select(i => i.Name)));

        }

        [Test]
        public void CanDoACountWithAFilter()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            
            //When a Contains method is called for a specific item 
            var items = pq.Where(s => s.Name.Contains("07"));

            //Then the result should contain a single item
            var count = items.Count();
            Assert.AreEqual(1, count);
            //And the item should be that item
            Assert.AreEqual("Item 07", items.Single().Name);
        }

        [Test]
        public void CanDoACountWithANullComparison()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name != null);
            var count = items.Count();

            Assert.AreEqual(10, count);
        }


        [Test]
        public void CanDoACountWithNoIllEffect()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var count = pq.Count();

            Assert.AreEqual(10, count);
        }

        [Test]
        public void CanDoATakeWithNoIllEffectOnOtherQueries()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var somethingElse = pq.Take(5);
            Assert.AreEqual(5, somethingElse.Count());

            Assert.AreEqual(10, pq.Count());
        }


        [Test]
        public void CanDoASelectWithNoIllEffectOnOtherQueries()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource).Select(e => new SomeEntityVm()
            {
                Name = e.Name
            });
            Assert.AreEqual(5, pq.Take(5).Count());
            Assert.AreEqual(10, pq.Count());
        }





        [Test]
        public void CanWorkWithoutQuery()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var items = pq.ToArray();

            Assert.AreEqual(0, Skipped);
            Assert.AreEqual(null, Taken);

            Assert.AreEqual("Item 01,Item 02", string.Join(",", items.Take(2).Select(i => i.Name)));

        }

        [Test]
        public void CanHandleAProjection()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Select(s => new Projection() { Item = s.Name });
            Assert.AreEqual("Item 01", items.ToArray().First().Item);
        }

        [Test]
        public void CanHandleAMethodCallWhereClause()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.Contains("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleAnOperatorWhereClause()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }


        [Test]
        public void CanHandleAnOperatorWhereClauseAgainstAVariable()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var variable = "Item 07";
            var items = pq.Where(s => s.Name == variable);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }
        [Test]
        public void CanHandleASecondWhereClauseAfterACount()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            var count = items.Count();
            var items2 = items.Where(s => s.Name != "Item 07");
            Assert.AreEqual(0, items2.ToArray().Length);
            
        }


        [Test]
        public void CanHandleAnOperatorWhereClauseOnAValueType()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Index != 0 && s.Index == 7);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }



        [Test]
        public void CanHandleAnAndAlsoWhereClause()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" && s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleAnOrElseWhereClause()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" || s.Name == "Item 08");
            Assert.AreEqual("Item 07", items.ToArray().First().Name);
            Assert.AreEqual("Item 08", items.ToArray().Skip(1).First().Name);
        }


        [Test]
        public void CanHandleAnEndsWithMethodCallWhereClause()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.EndsWith("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleASkipATakeAndAProjection()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }
        [Test]
        public void CanHandleAProjectionASkipAndATake()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }

        [Test]
        public void CanHandleAProjectionAndACount()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq
                .Where(i => i.Name.Contains("07"));
            var projection = someEntities
                .Select(s => new SomeEntityVm() { Name = s.Name })
                .Where(p => p.Name == "Item 07");
            var itemCount = projection.Count();
            Assert.AreEqual(1, itemCount);
        }


        [Test]
        public void CanHandleAProjectionAndACountAgainstIncompleteProvider()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = (info) => IncompleteDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq
                .Where(i => i.Name.Contains("07"));
            var projection = someEntities
                .Select(s => new Projection { Item = s.Name });
            var itemCount = projection.Count();
            Assert.AreEqual(10, itemCount);
        }

        [Test, Ignore("Not implemented")]
        public void CanHandleAProjectionAndACountAgainstLambdaProvider()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = (info) => LambdaDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq;
            var projection = someEntities
                .Select(s => new Projection { Item = s.Name })
                .Where(i => i.Item.Contains("07"));
            ;
            var itemCount = projection.Count();
            Assert.AreEqual(1, itemCount);
        }


        [Test]
        public void CanHandleAProjectionASkipAndAnOrderByDesc()
        {
            //Given an IQueryable made against a simple datasource
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = SomeDataAccessMethod;
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.OrderByDescending(e => e.Name).Skip(1).Take(1)
                .Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 09", items.ToArray().First().Item);
        }

        [Test]
        public void CanHandleAProjectionASkipAndAnOrderByAsc()
        {
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = (info) =>
            {
                Assert.AreEqual(OrderBy.OrderByDirection.Asc, info.OrderBy.Direction);
                return Enumerable.Empty<SomeEntity>();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            pq.OrderBy(e => e.Name).ToArray();
        }
        [Test]
        public void CanDoAnOptimizedCount()
        {
            Func<QueryInfo, IEnumerable<SomeEntity>> getPageFromDataSource = (info) =>
            {
                throw new NotImplementedException();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource, qi => 15);
            Assert.AreEqual(15, pq.Count(x => x.Index > 1));
        }
        [Test]
        public void CanApplyAQueryInfo()
        {
            var queryable = Enumerable.Range(1, 100)
                .Select(i => new SomeEntity()
                {
                    Name = "User" + i, Index = i
                })
                .ToArray().AsQueryable();


            var pq = new DelegateQueryable<SomeEntity>((info) => info.ApplyTo(queryable));

            Assert.AreEqual(90, pq.OrderByDescending(o => o.Index).Skip(10).Take(1).Single().Index);
        }



        

        // this method could call a sproc, or a webservice etc.
        static IEnumerable<SomeEntity> SomeDataAccessMethod(QueryInfo qi)
        {
            Skipped = qi.Skip;
            Taken = qi.Take;
            var query = Data.AsQueryable();
            if (qi.OrderBy != null)
            {
                var clause = qi.OrderBy.Name;
                if (qi.OrderBy.Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                var where = clause as Where;
                if (where != null)
                {
                    if (where.Operator == "Contains" || where.Operator == "EndsWith")
                    {
                        query = query.Where(where.PropertyName + "." + clause.Operator + "(@0)", where.Value);
                    }
                    if (clause.Operator == "Equal")
                    {
                        query = query.Where(where.PropertyName + " == @0", where.Value);
                    }

                    if (clause.Operator == "NotEqual")
                    {
                        query = query.Where(where.PropertyName + " != @0", where.Value);
                    }


                }
                
                if (clause.Operator == "OrElse")
                {
                    query = query.Where((Expression<Func<SomeEntity, bool>>) ((Or)clause).Expression);
                }
                
            }

            query = query.Skip(qi.Skip);
            if (qi.Take != null) query = query.Take(qi.Take.Value);
            return query.ToArray();
        }

        static IEnumerable<SomeEntity> LambdaDataSource(QueryInfo qi)
        {
            Skipped = qi.Skip;
            Taken = qi.Take;
            var query = Data.AsQueryable();
            if (qi.OrderBy != null)
            {
                var clause = qi.OrderBy.Name;
                if (qi.OrderBy.Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                query = query.Where((Expression<Func<SomeEntity, bool>>) clause.Expression);

            }

            query = query.Skip(qi.Skip);
            if (qi.Take != null) query = query.Take(qi.Take.Value);
            return query.ToArray();
        }

        static IEnumerable<SomeEntity> IncompleteDataSource(QueryInfo qi)
        { 
            var query = Data.AsQueryable();
            return query.ToArray();
        }

    }

    public class SomeEntityVm
    {
        public string Name { get; set; }
    }

    public   class SomeEntity
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class Projection
    {
        public string Item { get; set; }
    }
}
