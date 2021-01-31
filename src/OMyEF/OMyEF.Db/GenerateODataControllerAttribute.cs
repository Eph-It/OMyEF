using System;
namespace OMyEF.Db
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateODataControllerAttribute : Attribute
    {
        public GenerateODataControllerAttribute()
        {
        }
        public bool Authorize { get; set; }
        public string AuthorizePolicy { get; set; }
        public string AuthorizeRoles { get; set; }
        public string SetName { get; set; }
    }
}