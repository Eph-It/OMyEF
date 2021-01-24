using System;
using System.Collections.Generic;
using System.Text;

namespace OMyEF.Db
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateODataRouteAttribute : Attribute
    {
        public GenerateODataRouteAttribute() { }
        public string BaseRoute { get; set; } = "odata";
    }
}
