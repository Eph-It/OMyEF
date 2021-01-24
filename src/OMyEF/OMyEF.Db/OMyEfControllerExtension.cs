using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMyEF.Db
{
    public abstract class OMyEfControllerExtension
    {
        /// <summary>
        /// Can be overridden to perform a "soft" delete and/or skip the delete
        /// Simply return false and the object will not be deleted. Change a property (like IsDeleted)
        /// to false to perform a "soft" delete. The controller will always save the DbContext if there was an edit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool CanDelete<T>(T obj)
        {
            return true;
        }
        /// <summary>
        /// Run before the get query. Can be overridden to add custom filtering
        /// like IsDeleted == false or custom security (just throw a 401 exception to stop the query)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public virtual IQueryable<T> BeforeGet<T>(IQueryable<T> queryable)
        {
            return queryable;
        }
        /// <summary>
        /// Run before an edit happens - can be used to add custom security or add additional
        /// modifications, like populating "LastModifiedBy" properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public virtual T BeforeEdit<T>(T obj)
        {
            return obj;
        }
        /// <summary>
        /// Run after an edit successfully happens - can be used to add in additional logic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public virtual void AfterEdit<T>(T obj)
        {
            
        }
        /// <summary>
        /// Run before inserting - can be used to modify properties and add additional security logic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T BeforeInsert<T>(T obj)
        {
            return obj;
        }
        /// <summary>
        /// Run after a successful insert - can be used for additional logic like logging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public virtual void AfterInsert<T>(T obj)
        {
            
        }
        /// <summary>
        /// Run before deleting - can be used to modify properties and add additional security logic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T BeforeDelete<T>(T obj)
        {
            return obj;
        }
        /// <summary>
        /// Run after a successful delete - can be used for additional logic like logging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public virtual void AfterDelete<T>(T obj)
        {

        }
    }
    public class OMyEfControllerExtensionImplementation : OMyEfControllerExtension
    {

    }
}
