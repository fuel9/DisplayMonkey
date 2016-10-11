/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    public static class Constants
    {
        public const string PasswordMask = "****************";
        public const string EmailMask = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
    }

    #region Sundry


    public static class LinqExtension
    {
        public static IEnumerable<T> SelfAndChildren<T>(
            this T self,
            Func<T, IEnumerable<T>> children
            )
        {
            yield return self;
            IEnumerable<T> elements = children(self);
            foreach (T c in elements.SelectMany(e => e.SelfAndChildren(children)))
            {
                yield return c;
            }
        }
    }


    public partial class DisplayMonkeyEntities : DbContext
    {
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage)
                    ;

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                //var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                //throw new System.Data.Entity.Validation.DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                throw new System.Data.Entity.Validation.DbEntityValidationException(fullErrorMessage, ex.EntityValidationErrors);
            }
        }

        /*static void ObjectContext_SavingChanges(object sender, EventArgs args)
        {
            ObjectContext context = sender as ObjectContext;
            if (context != null)
            {
                foreach (ObjectStateEntry e in context.ObjectStateManager.GetObjectStateEntries(EntityState.Modified))
                {
                    Type type = e.Entity.GetType();
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        var test = property.GetCustomAttributes();
                        
                        var metaDataList = property.GetCustomAttributes(typeof(MetadataTypeAttribute), false);
                        foreach (MetadataTypeAttribute metaData in metaDataList)
                        {
                            properties = metaData.MetadataClassType.GetProperties();
                            foreach (PropertyInfo subproperty in properties)
                            {
                                var attributes = subproperty.GetCustomAttributes(typeof(NeverUpdateAttribute), false);
                                if (attributes.Length > 0)
                                    e.RejectPropertyChanges(property.Name);
                            }
                        }
                    }
                }
            }
        }*/
    }

    //public class NeverUpdateAttribute : System.Attribute
    //{
    //}

    #endregion
}