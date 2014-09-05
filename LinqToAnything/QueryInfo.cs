using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Linq.Dynamic;
namespace LinqToAnything
{
    /// <summary>
    /// an object representing a query. i.e.
    /// 
    ///     someQueryable
    ///         .Clause(x => x.Age >= 18)
    ///         .Clause(x => x.Name.Contains("John")
    ///         .OrderBy(x => x.Name).Skip(20).Take(10) 
    /// 
    /// would map to the values in the comments below
    /// </summary>


    public class QueryInfo
    {


        public QueryInfo()
        {
            Clauses = Enumerable.Empty<Clause>();
        }
        public int? Take { get; set; } //10
        public int Skip { get; set; } //20
        public OrderBy OrderBy { get; set; } //LinqToAnything.OrderBy.Asc
        public IEnumerable<Clause> Clauses { get; set; } // an array containing two where objects

        public QueryInfo Clone()
        {
            return new QueryInfo()
            {
                Take = this.Take,
                Skip = this.Skip,
                OrderBy = this.OrderBy == null ? null : this.OrderBy.Clone(),
                Clauses = this.Clauses.Select(c => c.Clone()).ToList()
            };
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> q)
        {
            var qi = this;
            foreach (var clause in qi.Clauses)
            {
                q = q.Where((Expression<Func<T, bool>>)clause.Expression);
            }
            if (qi.OrderBy != null)
            {
                var orderBy = qi.OrderBy.Name;
                if (this.OrderBy.Direction == OrderBy.OrderByDirection.Desc)
                    orderBy += " descending";
                q = q.OrderBy(orderBy);
            }

            if (qi.Skip > 0) q = q.Skip(qi.Skip);

            if (qi.Take != null) q = q.Take(qi.Take.Value);



            return q;
        }
    }


    public class Or : Clause
    {
        public IEnumerable<Clause> Clauses { get; set; }

        public override Clause Clone()
        {
            return new Or()
            {
                Operator = this.Operator,
                Expression = Expression,
                Clauses = this.Clauses.Select(c => c.Clone())
            };
        }

        public override string ToString()
        {
            return "(" + string.Join(") or ( ", Clauses.Select(c => c.ToString())) + ")";
        }
    }

    public class Where : Clause
    {
        public string PropertyName { get; set; }

        /// <summary>
        /// either a method name (e.g. Contains, StartsWith) or an operator name (op_Inequality,op_GreaterThan,op_GreaterThanOrEqual,op_LessThan,op_LessThanOrEqual,op_Multiply,op_Subtraction,op_Addition,op_Division,op_Modulus,op_BitwiseAnd,op_BitwiseOr,op_ExclusiveOr)
        /// </summary>
        public object Value { get; set; }


        public override Clause Clone()
        {
            return new Where()
            {
                Operator = this.Operator,
                PropertyName = this.PropertyName,
                Expression = Expression,
                Value = this.Value
            };
        }

        public override string ToString()
        {
            return "where " + PropertyName + " " + Operator + " " + Value.ToString();
        }
    }

    public abstract class Clause
    {
        public string Operator { get; set; }
        public Expression Expression { get; set; }
        public abstract Clause Clone();
    }

    public class OrderBy
    {
        public OrderBy(string name, OrderByDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public enum OrderByDirection
        {
            Asc,
            Desc
        }

        public string Name { get; set; }
        public OrderByDirection Direction { get; set; }

        public OrderBy Clone()
        {
            return new OrderBy(this.Name, this.Direction);
        }

        public override string ToString()
        {
            return "order by " + Name + " " + this.Direction.ToString().ToUpper();
        }
    }

    public static class QueryInfoExtension
    {
        public static T GetWhereClauseValue<T>(this QueryInfo qi, string propertyName, string @operator)
        {
            return qi.Clauses.OfType<Where>()
                .Where(c => c.PropertyName == propertyName && c.Operator == @operator)
                .Select(c => c.Value)
                .OfType<T>()
                .SingleOrDefault();
        }
    }
}