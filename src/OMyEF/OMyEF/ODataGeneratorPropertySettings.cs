using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace OMyEF
{
    public class ODataGeneratorPropertySettings
    {
        public ODataGeneratorPropertySettings(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol, Dictionary<string,ISymbol> AttributeDictionary)
        {
            
            var type = (INamedTypeSymbol)propertySymbol.Type;
            ITypeSymbol typeSymbol = type.TypeArguments.FirstOrDefault();
            if(typeSymbol == null)
            {
                return;
            }
            PropertyType = typeSymbol.ToString();
            PropertyName = propertySymbol.Name;
            var members = type.GetMembers();
            foreach(var m in members)
            {
                var mName = m.Name.ToString();
                var mSymbolKind = m.Kind.ToString();
                if(m.Kind == SymbolKind.Property)
                {
                    var attributes = m.GetAttributes();
                    foreach(var a in attributes)
                    {
                        Console.WriteLine(a.ToString());
                    }
                }
            }
        }
        public string DbContextName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string KeyName { get; set; }
        public string KeyType { get; set; }
        public string ControllerHeader
        {
            get
            {
                return $@"
                    [ApiController]
                    public class {PropertyName}Controller : ControllerBase
                    {{
                        private {DbContextName} _dbContext;
                        public {PropertyName}Controller({DbContextName} dbContext){{
                            _dbContext = dbContext;
                        }}

                ";
            }
        }
        public string ControllerFooter
        {
            get
            {
                return "}";
            }
        }
        private string oDataGetKey
        {
            get
            {
                if(String.IsNullOrEmpty(KeyName) || String.IsNullOrEmpty(KeyType))
                {
                    return "";
                }
                return $@"
                    [HttpGet]
                    [EnableQuery]
                    [Route(""odata/[controller]"")]
                    public SingleResult<{PropertyType}> Get([FromODataUri] {KeyType} key)
                    {{
                        return SingleResult.Create(_dbContext.{PropertyName}.Where(p => p.{KeyName} == key));
                    }}
                ";
            }
        }
        public string ODataGet
        {
            get
            {
                return $@"
                    [HttpGet]
                    [EnableQuery]
                    [Route(""odata/[controller]"")]
                    public IQueryable<{PropertyType}> Get()
                    {{
                        return _dbContext.{PropertyName};
                    }}
                ";
            }
        }
    }
}
