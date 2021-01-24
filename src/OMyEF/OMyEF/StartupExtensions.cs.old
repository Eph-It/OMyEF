#if !NETSTANDARD2_0
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Builder;
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
            routeBuilder.MapODataRoute("odata", "odata", GetEdmModel<T>());
        }
        private static IEdmModel GetEdmModel<T>()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            foreach(var prop in typeof(T).GetProperties())
            {
                bool addProp = false;
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if(attr.GetType().Name == "GenerateODataControllerAttribute")
                    {
                        addProp = true;
                    }
                }
                if(addProp)
                {
                    var method = typeof(ODataConventionModelBuilder).GetMethod(nameof(ODataConventionModelBuilder.EntitySet));
                    var genericMethod = method.MakeGenericMethod(prop.PropertyType.GenericTypeArguments[0]);
                    genericMethod.Invoke(odataBuilder, new[] { prop.Name });
                }
            }
            return odataBuilder.GetEdmModel();
        }
    }
}
#endif