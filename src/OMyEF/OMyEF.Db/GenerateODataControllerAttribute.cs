using System;
namespace OMyEF.Db
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateODataControllerAttribute : Attribute
    {
        public GenerateODataControllerAttribute()
        {
        }
    }
}