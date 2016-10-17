using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

using Psns.Common.Persistence.Definitions;

namespace Psns.Common.Persistence.EntityFramework
{
    /// <summary>
    /// An implementation of IRepository for Entity Framework
    /// </summary>
    /// <typeparam name="T">A reference type</typeparam>
    public class Repository<T> : IRepository<T>, IDisposable where T : class, IIdentifiable
    {
        /// <summary>
        /// The DbContext being used for all actions
        /// </summary>
        protected DbContext _dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext">The DbContext for the project</param>
        /// <exception cref="System.InvalidOperationException">When dbContext doesn't implement IContext</exception>
        public Repository(DbContext dbContext)
        {
            if(!(dbContext is IContext))
                throw new InvalidOperationException("dbContext doesn't implement IContext");

            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all entities of type T as an untracked list
        /// </summary>
        /// <param name="includes">Property names to be included</param>
        /// <returns>An IEnumerable of all of the entities</returns>
        public virtual IQueryable<T> All(params string[] includes)
        {
            return _dbContext.Set<T>().IncludeMany(includes).AsNoTracking();
        }

        /// <summary>
        /// Add a new entity to DbContext
        /// </summary>
        /// <param name="entity">The entity to be added</param>
        /// <returns>The newly created entity</returns>
        public virtual T Create(T entity)
        {
            return _dbContext.Set<T>().Add(entity);
        }

        /// <summary>
        /// Update the given entity in DbContext
        /// </summary>
        /// <param name="entity">The entity containing updated property</param>
        /// <param name="includes">The complex properties that should be included in the Update</param>
        /// <returns>The entity containing updates</returns>
        /// <exception cref="System.InvalidOperationException">When the entity doesn't have a keyed property (i.e. an integer Id or a property with the KeyAttribute)</exception>
        /// <exception cref="System.InvalidOperationException">When the existing entity can't be found by using the key</exception>
        /// <exception cref="System.InvalidOperationException">If the included collection property of the entity doesn't implement Add or Remove</exception>
        public virtual T Update(T entity, params string[] includes)
        {
            var key = GetKeyValue(entity);

            var old = Find(key);
            if(old == null)
                throw new InvalidOperationException(string.Format("Entity with key {0} could not be found", key));

            if(includes != null && includes.Length > 0)
            {
                RelationshipLoaderAdapter.Load(_dbContext, old, includes);

                foreach(var property in entity.GetEnumerableProperties()
                    .Where(p => includes.Contains(p.Name)))
                {
                    var newCollection = property.GetValue(entity, null) as ICollection;
                    var oldCollection = old.GetType()
                        .GetProperty(property.Name)
                        .GetValue(old, null) as ICollection;

                    var add = oldCollection.GetType().GetMethod("Add");
                    var remove = oldCollection.GetType().GetMethod("Remove");

                    if(add == null || remove == null)
                        throw new InvalidOperationException(string.Format("{0} must implement both Add and Remove methods", property.Name));

                    var comparer = new IdentifiableComparer();
                    var newIdentifiables = newCollection.OfType<IIdentifiable>();
                    var oldIdentifiables = oldCollection.OfType<IIdentifiable>();

                    var additions = newIdentifiables
                        .Where(item => !oldIdentifiables.Contains(item, comparer)).ToList();
                    foreach(var newItem in additions)
                    {
                        add.Invoke(oldCollection, new object[] { newItem });
                    };

                    var removals = oldIdentifiables
                        .Where(item => !newIdentifiables.Contains(item, comparer)).ToList();
                    foreach(var toDelete in removals)
                    {
                        remove.Invoke(oldCollection, new object[] { toDelete });
                    };
                }
            }

            (_dbContext as Context).SetModified(old, entity);

            return entity;
        }

        private static object GetKeyValue(T entity)
        {
            object key = entity.Id;

            var keyProperty = entity.GetType().GetProperties().Where(p => (p.GetCustomAttributes(typeof(KeyAttribute), false) as KeyAttribute[]).Length > 0).SingleOrDefault();
            if(keyProperty != null)
            {
                key = keyProperty.GetValue(entity, null);
            }

            return key;
        }

        /// <summary>
        /// Find a single entity
        /// </summary>
        /// <param name="keyValues">Key values to be used in the search</param>
        /// <returns>The found entity or null</returns>
        public virtual T Find(params object[] keyValues)
        {
            return _dbContext.Set<T>().Find(keyValues);
        }

        /// <summary>
        /// Find a single entity asynchronously
        /// </summary>
        /// <param name="keyValues">Key values to be used in the search</param>
        /// <returns>The found entity or null</returns>
        public async virtual Task<T> FindAsync(params object[] keyValues)
        {
            return await _dbContext.Set<T>().FindAsync(keyValues);
        }

        /// <summary>
        /// Find all entities that match the given predicate
        /// </summary>
        /// <param name="predicate">The predicate query</param>
        /// <param name="includes">The properties to be included in the results</param>
        /// <returns>The list of matching entites</returns>
        public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate, params string[] includes)
        {
            return _dbContext.Set<T>().IncludeMany(includes).Where(predicate).AsNoTracking();
        }

        /// <summary>
        /// Remove the entity from DbContext
        /// </summary>
        /// <param name="entity">The entity to be removed</param>
        /// <returns>The removed entity</returns>
        public virtual T Delete(T entity)
        {
            return _dbContext.Set<T>().Remove(entity);
        }

        /// <summary>
        /// Calls DbContext.SaveChanges
        /// </summary>
        /// <returns>The count of entities modified</returns>
        public virtual int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// Calls DbContext.SaveChangesAsync
        /// </summary>
        /// <returns>The count of entities modified</returns>
        public async virtual Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        #region IDisposable
        /// <summary>
        /// Destructor
        /// </summary>
        ~Repository()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the underlying DbContext
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the underlying DbContext
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                    _dbContext = null;
                }
            }
        }
        #endregion
    }
}
