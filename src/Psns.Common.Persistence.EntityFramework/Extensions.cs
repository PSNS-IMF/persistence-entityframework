using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Psns.Common.Persistence.EntityFramework
{
    /// <summary>
    /// Some extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get all properties of the object that implement IEnumerable
        /// </summary>
        /// <param name="obj">this object</param>
        /// <returns>The PropertyInfos that implement IEnumerable</returns>
        public static IEnumerable<PropertyInfo> GetEnumerableProperties(this object obj)
        {
            return obj
                .GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                    p.PropertyType.GetInterfaces().Any(x => x == typeof(IEnumerable)));
        }

        /// <summary>
        /// Calls IQueryable.Include on multiple string values
        /// </summary>
        /// <typeparam name="T">T in context</typeparam>
        /// <param name="query">this IQueryable</param>
        /// <param name="includes">String parameters in be included</param>
        /// <returns>The IQueryable containing all includes</returns>
        public static IQueryable<T> IncludeMany<T>(this IQueryable<T> query,
            params string[] includes)
            where T : class
        {
            if(includes != null)
            {
                foreach(var include in includes)
                    query = query.Include<T>(include);
            }

            return query;
        }
    }
}
