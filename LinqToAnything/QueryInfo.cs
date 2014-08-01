using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        public int? Take { get; internal set; } //10
        public int Skip { get; internal set; } //20
        public OrderBy OrderBy { get; internal set; } //LinqToAnything.OrderBy.Asc
        public IEnumerable<Clause> Clauses { get; internal set; } // an array containing two where objects

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
    }


    public class Or : Clause
    {
        public IEnumerable<Clause> Clauses { get; internal set; }

        public override Clause Clone()
        {
            return new Or()
            {
                Operator = this.Operator,
                Expression = Expression,
                Clauses = this.Clauses.Select(c => c.Clone())
            };
        }
    }

    public class Where : Clause
    {
        public string PropertyName { get; internal set; }

        /// <summary>
        /// either a method name (e.g. Contains, StartsWith) or an operator name (op_Inequality,op_GreaterThan,op_GreaterThanOrEqual,op_LessThan,op_LessThanOrEqual,op_Multiply,op_Subtraction,op_Addition,op_Division,op_Modulus,op_BitwiseAnd,op_BitwiseOr,op_ExclusiveOr)
        /// </summary>
        public object Value { get; internal set; }


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
    }

    public abstract class Clause
    {
        public string Operator { get; internal set; }
        public Expression Expression { get; internal set; }
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

        public string Name { get; internal set; }
        public OrderByDirection Direction { get; internal set; }

        public OrderBy Clone()
        {
            return new OrderBy(this.Name, this.Direction);
        }
    }
}