using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LinqToAnything.Tests
{

    public class IndexExtensionTests
    {
        [Test]
        public void TestForIndexExtension()
        {
            var users = new List<User>()
            {
                new User {UserId = 123},
                new User {UserId = 456},
            };

            var friendships = new List<Friendship>()
            {
                new Friendship {UserId = 123, FriendUserId = 456}
            }.AsQueryable()
                .IndexOn(fr => fr.UserId);

            var joined =
                users.Select(user => new
                {
                    user,
                    friends =
                        friendships.Where(fr => user.UserId == fr.FriendUserId).Select(x => x.FriendUserId).ToArray()
                });

            joined.ToArray();
        }

        [Test]
        public void TestForIndexExtension2()
        {
            var users = new List<User>()
            {
                new User {UserId = 123, Role = "m", Team = "p"},
                new User {UserId = 456, Role = "j", Team = "p"},
                new User {UserId = 456, Role = "j", Team = "q"},
                new User {UserId = 789, Role = "j", Team = "q"},
                new User {UserId = 012, Role = "m", Team = "1"},
            }.AsQueryable().AutoIndex();

            var filtered = users.Where(u => u.Team == "p").Where(u => u.Role == "m");

            Assert.AreEqual(1, filtered.ToArray().Count());
        }
        [Test]
        public void GroupTest()
        {
            var users = new List<User>()
            {
                new User {UserId = 123, Role = "m", Team = "p"},
                new User {UserId = 456, Role = "j", Team = "p"},
                new User {UserId = 456, Role = "j", Team = "q"},
                new User {UserId = 789, Role = "j", Team = "q"},
                new User {UserId = 012, Role = "m", Team = "1"},
            }.AsQueryable().AutoIndex();

            var groups = users.GroupBy(u => u.Team);

            Assert.AreEqual(3, groups.ToArray().Count());
            var teamP = groups.Single(g => g.Key == "p");
            Assert.AreEqual(2, teamP.Count());

        }


        class User
        {
            public int UserId { get; set; }
            public string Role { get; set; }
            public string Team { get; set; }
        }

        class Friendship
        {
            public int UserId { get; set; }
            public int FriendUserId { get; set; }
        }


    }
    public static class IndexOnColumnExtension
    {
        public static IQueryable<T> IndexOn<T, TKey>(this IQueryable<T> items,
            Expression<Func<T, TKey>> propertySelectorExp)
        {
            var memberAccess = (MemberExpression)propertySelectorExp.Body;
            var memberName = memberAccess.Member.Name;
            var lambda = propertySelectorExp.Compile();
            var lookup = items.ToLookup(x => default(TKey), t => t);


            return new DelegateQueryable<T>(qi =>
            {
                var whereClause =
                    qi.Clauses.OfType<Where>()
                        .FirstOrDefault(c => c.PropertyName == memberName && c.Operator == "Equal");
                if (whereClause != null)
                {
                    var filteredItems = lookup[(TKey)whereClause.Value].AsQueryable();
                    qi.Clauses = qi.Clauses.Except(new[] { whereClause });
                    return qi.ApplyTo(filteredItems);
                }
                return qi.ApplyTo(items);
            });

        }
    }
}