using System;
using System.Collections.Generic;
using System.Text;

namespace OMyEF
{
    public class OMyEFControllerBuilder
    {
        public string ControllerClassName
        {
            get
            {
                return DbSetPropertyName + "Controller";
            }
        }
        public string ControllerNamespace { get; set; } = "OMyEF.GeneratedControllers";
        public string BaseRoute { get; set; }
        public string KeyName { get; set; }
        public string KeyType { get; set; }
        public string DbContextNamespace { get; set; }
        public string DbContextType { get; set; }
        public string DbSetNamespace { get; set; }
        public string DbSetPropertyName { get; set; }
        public string DbSetPropertyType { get; set; }
        public bool Authorize { get; set; }
        public string AuthorizePolicy { get; set; }
        public string AuthorizeRoles { get; set; }

        private void AddHeader(StringBuilder stringBuilder)
        {
            stringBuilder.Append(@"
                using Microsoft.AspNetCore.Mvc;
                using System;
                using System.Linq;
                using System.Net;
                using Microsoft.AspNet.OData;
                using Microsoft.AspNet.OData.Routing;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                using System.Text;
                using OMyEF.Db;
                using OMyEF.Server;
                using Microsoft.AspNetCore.Authorization;
            ");
            if(DbSetNamespace != null)
            {
                stringBuilder.AppendLine($"using {DbSetNamespace};");
            }
            if(DbContextNamespace != null && DbContextNamespace != DbSetNamespace)
            {
                stringBuilder.AppendLine($"using {DbContextNamespace};");
            }
            stringBuilder.AppendLine(@$"
                namespace {ControllerNamespace}{{
                    [Route(""{BaseRoute}/[controller]"")]
                    public class {ControllerClassName} : ODataController {{
                        private {DbContextType} _dbContext;
                        private OMyEFControllerExtensions<{DbSetPropertyType}> _controllerExtension;
                        public {ControllerClassName}({DbContextType} dbContext, OMyEFControllerExtensions<{DbSetPropertyType}> controllerExtension = null){{
                            _dbContext = dbContext;
                            if(controllerExtension == null){{
                                _controllerExtension = new OMyEFControllerExtensionDefaultImplementation<{DbSetPropertyType}>();
                            }}
                            else {{ _controllerExtension = controllerExtension; }}
                            if(_controllerExtension.DbContext == null)
                            {{
                                _controllerExtension.DbContext = _dbContext;
                            }}
                        }}                        
            ");
        }

        private void AddGetActions(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@$"
                [HttpGet]
                [EnableQuery]
                {AddAuthorizeAttribute("Get")}
                [Route(""{BaseRoute}/[controller]"")]
                public IQueryable<{DbSetPropertyType}> Get()
                {{
                    return _controllerExtension.Get();
                }}
            ");
            if(!String.IsNullOrEmpty(KeyName) && !String.IsNullOrEmpty(KeyType))
            {
                stringBuilder.AppendLine($@"
                    [HttpGet]
                    {AddAuthorizeAttribute("Get")}
                    [Route(""{BaseRoute}/[controller]"")]
                    public {DbSetPropertyType} Get([FromODataUri] {KeyType} key)
                    {{
                        return _controllerExtension.Get(key);
                    }}
                ");
            }
        }

        private void AddPostActions(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@$"
                [HttpPost]
                {AddAuthorizeAttribute("Post")}
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Post([FromBody] {DbSetPropertyType} item)
                {{
                    if (!ModelState.IsValid){{ return BadRequest(ModelState); }}
                    
                    item = await _controllerExtension.PostAsync(item);

                    return Ok(item);
                }}
            ");
        }

        private void AddPatchAction(StringBuilder stringBuilder)
        {
            if (String.IsNullOrEmpty(KeyName) && String.IsNullOrEmpty(KeyType)) { return; }
            stringBuilder.AppendLine(@$"
                [HttpPatch]
                {AddAuthorizeAttribute("Patch")}
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Patch([FromODataUri] {KeyType} key, Delta<{DbSetPropertyType}> item)
                {{
                    if (!ModelState.IsValid)
                    {{
                        return BadRequest(ModelState);
                    }}
                    var entity = await _controllerExtension.PatchAsync(item, key);

                    if (entity == null)
                    {{
                        return NotFound();
                    }}

                    return Ok(entity);
                }}
            ");
        }

        private void AddPutAction(StringBuilder stringBuilder)
        {
            // if we don't know what the key is, we can't add an edit
            if (String.IsNullOrEmpty(KeyName) && String.IsNullOrEmpty(KeyType)) { return; }
            stringBuilder.AppendLine(@$"
                [HttpPut]
                {AddAuthorizeAttribute("Put")}
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Put([FromODataUri] {KeyType} key, {DbSetPropertyType} item)
                {{
                    if (!ModelState.IsValid)
                    {{
                        return BadRequest(ModelState);
                    }}
                    if (key != item.{KeyName})
                    {{
                        return BadRequest();
                    }}
                    var entity = await _controllerExtension.PutAsync(item);

                    return Ok(entity);
                }}
            ");
        }
        private void AddDeleteAction(StringBuilder stringBuilder)
        {
            // if we don't know what the key is, we can't add an edit
            if (String.IsNullOrEmpty(KeyName) && String.IsNullOrEmpty(KeyType)) { return; }
            stringBuilder.AppendLine($@"
                [HttpDelete]
                {AddAuthorizeAttribute("Delete")}
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Delete([FromODataUri] {KeyType} key)
                {{
                    try
                    {{
                        await _controllerExtension.DeleteAsync(key);
                    }}
                    catch (KeyNotFoundException)
                    {{
                        return NotFound();
                    }}
                    
                    return StatusCode((int)HttpStatusCode.NoContent);
                }}
            ");
        }

        private void AddFooter(StringBuilder stringBuilder)
        {
            stringBuilder.Append("}}");
        }

        private string AddAuthorizeAttribute(string action)
        {
            if (!Authorize) { return ""; }
            var authAttribute = "[Authorize(";
            if (!String.IsNullOrEmpty(AuthorizeRoles))
            {
                authAttribute = $"{authAttribute}Roles = \"{AuthorizeRoles}\"";
            }
            else if (!String.IsNullOrEmpty(AuthorizePolicy))
            {
                authAttribute = $"{authAttribute}Policy = \"{AuthorizePolicy}\"";
            }
            return authAttribute + ")]";
        }

        public string Build()
        {
            var stringBuilder = new StringBuilder();
            AddHeader(stringBuilder);
            AddGetActions(stringBuilder);
            AddPostActions(stringBuilder);
            AddPatchAction(stringBuilder);
            AddPutAction(stringBuilder);
            AddDeleteAction(stringBuilder);
            AddFooter(stringBuilder);
            return stringBuilder.ToString();
        }
    }
}
