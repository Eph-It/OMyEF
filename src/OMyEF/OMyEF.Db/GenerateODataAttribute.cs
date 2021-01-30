using System;
using System.Collections.Generic;
using System.Text;

namespace OMyEF.Db
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateODataAttribute : Attribute
    {
        public GenerateODataAttribute() { }
        public string BaseRoute { get; set; } = "odata";
        public bool Authorize { get; set; }
        public string AuthorizePolicy { get; set; }
        public string AuthorizeRoles { get; set; }
    }
}
