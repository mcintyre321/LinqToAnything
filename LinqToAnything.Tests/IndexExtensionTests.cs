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


        class User
        {
            public int UserId { get; set; }
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