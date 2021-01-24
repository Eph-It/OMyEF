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
                        private OMyEfControllerExtension _controllerExtension;
                        public {ControllerClassName}({DbContextType} dbContext,OMyEfControllerExtension controllerExtension = null){{
                            _dbContext = dbContext;
                            if(controllerExtension == null){{
                                _controllerExtension = new OMyEfControllerExtensionImplementation();
                            }}
                            else {{ _controllerExtension = controllerExtension; }}
                        }}                        
            ");
        }

        private void AddGetActions(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@$"
                [HttpGet]
                [EnableQuery]
                [Route(""{BaseRoute}/[controller]"")]
                public IQueryable<{DbSetPropertyType}> Get()
                {{
                    return _controllerExtension.BeforeGet(_dbContext.{DbSetPropertyName});
                }}
            ");
            if(!String.IsNullOrEmpty(KeyName) && !String.IsNullOrEmpty(KeyType))
            {
                stringBuilder.AppendLine($@"
                    [HttpGet]
                    [Route(""{BaseRoute}/[controller]"")]
                    public {DbSetPropertyType} Get([FromODataUri] {KeyType} key)
                    {{
                        var dbQuery = _controllerExtension.BeforeGet(_dbContext.{DbSetPropertyName});
                        return dbQuery.Where(p => p.{KeyName} == key).First();
                    }}
                ");
            }
        }

        private void AddPostActions(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@$"
                [HttpPost]
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Post([FromBody] {DbSetPropertyType} item)
                {{
                    if (!ModelState.IsValid){{ return BadRequest(ModelState); }}
                    item = _controllerExtension.BeforeInsert(item);
                    _dbContext.{DbSetPropertyName}.Add(item);
                    await _dbContext.SaveChangesAsync();
                    _controllerExtension.AfterInsert(item);
                    return Ok(item);
                }}
            ");
        }

        private void AddPatchAction(StringBuilder stringBuilder)
        {
            if (String.IsNullOrEmpty(KeyName) && String.IsNullOrEmpty(KeyType)) { return; }
            stringBuilder.AppendLine(@$"
                [HttpPatch]
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Patch([FromODataUri] {KeyType} key, Delta<{DbSetPropertyType}> item)
                {{
                    if (!ModelState.IsValid)
                    {{
                        return BadRequest(ModelState);
                    }}
                    var entity = await _dbContext.{DbSetPropertyName}.FindAsync(key);

                    if (entity == null)
                    {{
                        return NotFound();
                    }}

                    item.Patch(entity);
                    entity = _controllerExtension.BeforeEdit(entity);
                    await _dbContext.SaveChangesAsync();
                    _controllerExtension.AfterEdit(entity);
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
                    _dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    
                    item = _controllerExtension.BeforeEdit(item);
                    await _dbContext.SaveChangesAsync();
                    _controllerExtension.AfterEdit(item);
                    return Ok(item);
                }}
            ");
        }
        private void AddDeleteAction(StringBuilder stringBuilder)
        {
            // if we don't know what the key is, we can't add an edit
            if (String.IsNullOrEmpty(KeyName) && String.IsNullOrEmpty(KeyType)) { return; }
            stringBuilder.AppendLine($@"
                [HttpDelete]
                [Route(""{BaseRoute}/[controller]"")]
                public async Task<IActionResult> Delete([FromODataUri] {KeyType} key)
                {{
                    var item = await _dbContext.{DbSetPropertyName}.FindAsync(key);
                    if (item == null)
                    {{
                        return NotFound();
                    }}
                    item = _controllerExtension.BeforeDelete(item);
                    if(_controllerExtension.CanDelete(item)){{
                        _dbContext.Remove(item);
                    }}
                    
                    await _dbContext.SaveChangesAsync();
                    _controllerExtension.AfterDelete(item);
                    return StatusCode((int)HttpStatusCode.NoContent);
                }}
            ");
        }

        private void AddFooter(StringBuilder stringBuilder)
        {
            stringBuilder.Append("}}");
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
