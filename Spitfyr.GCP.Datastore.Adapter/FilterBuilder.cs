using System;
using System.Linq.Expressions;
using Google.Cloud.Datastore.V1;

namespace Spitfyr.GCP.Datastore.Adapter
{
    public static class FilterBuilder<TEntity>
    {
        public static Filter And(params Filter[] filters)
        {
            return Filter.And(filters);
        }

        public static Filter Equal<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        {
            return Filter.Equal(GetMemberName(field), value);
        }

        public static Filter LessThan<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        {
            return Filter.LessThan(GetMemberName(field), value);
        }

        public static Filter LessThanOrEqual<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        {
            return Filter.LessThanOrEqual(GetMemberName(field), value);
        }

        public static Filter GreaterThan<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        {
            return Filter.GreaterThan(GetMemberName(field), value);
        }

        public static Filter GreaterThanOrEqual<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        {
            return Filter.GreaterThanOrEqual(GetMemberName(field), value);
        }

        //public static Filter Count<TProp>(Expression<Func<TEntity, TProp>> field, Value value)
        //{
        //    var propName = GetMemberName(field);
            
        //    return Filter.Equal(propName, value);
        //}

        private static string GetMemberName<TProp>(Expression<Func<TEntity, TProp>> field)
        {
            var memberExpression = field.Body as MemberExpression;
            var propName = memberExpression?.Member.Name;
            return propName;
        }
    }
}
