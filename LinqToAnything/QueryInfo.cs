using System.Collections;
using System.Collections.Generic;
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


    public interface QueryInfo
    {
        int? Take { get; }                      //10
        int Skip { get; }                       //20
        OrderBy OrderBy { get; }                //LinqToAnything.OrderBy.Asc
        IEnumerable<Clause> Clauses { get; }      // an array containing two where objects
    }


    public class Or : Clause
    {
        public IEnumerable<Clause> Clauses { get; internal set; }
        public Expression Expression { get; set; }
    }

    public class Where : Clause 
    {
        public string PropertyName { get; internal set; }
        /// <summary>
        /// either a method name (e.g. Contains, StartsWith) or an operator name (op_Inequality,op_GreaterThan,op_GreaterThanOrEqual,op_LessThan,op_LessThanOrEqual,op_Multiply,op_Subtraction,op_Addition,op_Division,op_Modulus,op_BitwiseAnd,op_BitwiseOr,op_ExclusiveOr)
        /// </summary>
        public object Value { get; internal set; }
        public Expression Expression { get; internal set; }
    }

    public class Clause
    {
        public string Operator { get; internal set; }   
    }
}