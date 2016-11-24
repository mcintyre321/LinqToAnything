using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToAnything
{
    public class GroupBy  
    {
        public GroupBy Clone()
        {
            return new GroupBy()
            {
                KeySelector =  this.KeySelector
            };
        }

        public string KeyName => (this.KeySelector.LambdaBody as MemberExpression)?.Member.Name;
        public Type KeyType => GetMemberInfoType((this.KeySelector.LambdaBody as MemberExpression)?.Member);

        private Type GetMemberInfoType(MemberInfo member)
        {
            return member?.DeclaringType.GetProperty(member.Name).PropertyType;
        }

        public ExpressionUtils.SelectCallMatch KeySelector { get; set; }
    }
}