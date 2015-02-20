using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Psns.Common.Persistence.EntityFramework
{
    /// <summary>
    /// Provides a level of indirection for loading navigation properties to support loose coupling
    /// </summary>
    public static class RelationshipLoaderAdapter
    {
        /// <summary>
        /// The function called to load a list of properties
        /// </summary>
        public static Action<DbContext, object, string[]> LoadFunction;

        /// <summary>
        /// Calls the LoadFunction; if null, then DbEntityEntry.[MemberEntryType].Load will be called.
        /// </summary>
        /// <param name="context">The DbContext</param>
        /// <param name="entity">The entity whose properties will be loaded</param>
        /// <param name="includes">The list of properties to load</param>
        public static void Load(DbContext context, object entity, params string[] includes)
        {
            if(LoadFunction != null)
                LoadFunction.Invoke(context, entity, includes);
            else
            {
                foreach(var include in includes)
                {
                    var entry = context.Entry(entity);
                    var entryMember = entry.Member(include);

                    if(entryMember is DbReferenceEntry)
                        entry.Reference(include).Load();
                    else if(entryMember is DbCollectionEntry)
                        entry.Collection(include).Load();
                }
            }
        }
    }
}
