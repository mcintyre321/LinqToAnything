using System.Collections;
using System.Collections.Generic;

namespace LinqToAnything
{
    /// <summary>
    /// an object representing a query. i.e.
    /// 
    ///     someQueryable
    ///         .Where(x => x.Age >= 18)
    ///         .Where(x => x.Name.Contains("John")
    ///         .OrderBy(x => x.Name).Skip(20).Take(10) 
    /// 
    /// would map to the values in the comments below
    /// </summary>


    public interface QueryInfo
    {
        int? Take { get; }                      //10
        int Skip { get; }                       //20
        OrderBy OrderBy { get; }                //LinqToAnything.OrderBy.Asc
        IEnumerable<Where> Wheres { get; }      // an array containing two where objects
    }

    public class Where 
    {
        public string PropertyName { get; internal set; }
        /// <summary>
        /// either a method name (e.g. Contains, StartsWith) or an operator name (op_Inequality,op_GreaterThan,op_GreaterThanOrEqual,op_LessThan,op_LessThanOrEqual,op_Multiply,op_Subtraction,op_Addition,op_Division,op_Modulus,op_BitwiseAnd,op_BitwiseOr,op_ExclusiveOr)
        /// </summary>
        public string Operator { get; internal set; }   
        public object Value { get; internal set; }
    }
}