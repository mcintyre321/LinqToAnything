using System;
using System.Collections.Generic;

namespace DelegateQueryable
{
    internal sealed class SystemType
    {
        internal Type GetLinqElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;

            return ienum.GetGenericArguments()[0];
        }

        internal Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;

            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                    if (typeof(IEnumerable<>).MakeGenericType(arg).IsAssignableFrom(seqType))
                        return typeof(IEnumerable<>).MakeGenericType(arg);
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
                return FindIEnumerable(seqType.BaseType);

            if (seqType.GetInterfaces() != null && seqType.GetInterfaces().Length > 0)
            {
                foreach (Type iface in seqType.GetInterfaces())
                    if (FindIEnumerable(iface) != null) return FindIEnumerable(iface);

            }

            return null;
        }
    }
}