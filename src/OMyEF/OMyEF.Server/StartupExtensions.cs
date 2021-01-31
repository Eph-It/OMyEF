using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Builder;
using System.Linq;
using OMyEF.Db;
using System;
using System.Reflection;

namespace OMyEF
{
    public static class StartupExtensions
    {
        public static IODataBuilder AddOMyEF(this IServiceCollection services)
        {
            return services.AddOData();
        }
        public static void AddOMyEFRoute<T>(this IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.Select().Filter().OrderBy().Count().MaxTop(10);
            string routePrefixName = "odata";
            foreach(var att in typeof(T).GetCustomAttributes())
            {
                if(att is GenerateODataAttribute attrib)
                {
                    if (!String.IsNullOrEmpty(attrib.BaseRoute))
                    {
                        routePrefixName = attrib.BaseRoute;
                    }
                }
            }
            routeBuilder.MapODataRoute(routePrefixName, routePrefixName, GetEdmModel<T>());
        }
        private static IEdmModel GetEdmModel<T>()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            foreach (var prop in typeof(T).GetProperties())
            {
                string addPropName = prop.Name;
                bool addProp = false;
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Count() == 1)
                {
                    var baseType = prop.PropertyType.GenericTypeArguments[0];
                    object[] attrs = baseType.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        if (attr is GenerateODataControllerAttribute genAttr)
                        {
                            addProp = true;
                            if(!String.IsNullOrEmpty(genAttr.SetName))
                            {
                                addPropName = genAttr.SetName;
                            }
                        }
                    }
                }
                if (addProp)
                {

                    var method = typeof(ODataConventionModelBuilder).GetMethod(nameof(ODataConventionModelBuilder.EntitySet));
                    var genericMethod = method.MakeGenericMethod(prop.PropertyType.GenericTypeArguments[0]);
                    genericMethod.Invoke(odataBuilder, new[] { addPropName });
                }


            }
            return odataBuilder.GetEdmModel();
        }
    }
}
