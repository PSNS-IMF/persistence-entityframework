﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Entity;

namespace Psns.Common.Persistence.EntityFramework
{
    /// <summary>
    /// Defines extra re-direction methods for DbContext
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// Definies a method to set the modified state of an Entry
        /// </summary>
        /// <param name="entity">The entity to modify</param>
        /// <param name="updated">The entity containing updated values</param>
        void SetModified(object entity, object updated);
    }

    /// <summary>
    /// An abstract implementation of IContext
    /// </summary>
    public abstract class Context : DbContext, IContext
    {
        /// <summary>
        /// Sets Entry.State to EntityState.Modified
        /// </summary>
        /// <param name="entity">The entity to modify</param>
        /// <param name="updated">The entity containing updated values</param>
        public virtual void SetModified(object entity, object updated)
        {
            Entry(entity).CurrentValues.SetValues(updated);
        }
    }
}
