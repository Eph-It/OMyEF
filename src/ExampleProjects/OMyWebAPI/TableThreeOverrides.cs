using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OMyEF.Server;
using OMyEFDbContext;

namespace OMyWebAPI
{
    public class TableThreeOverrides : OMyEFControllerExtensions<TableThree>
    {
        public override Task<TableThree> PostAsync(TableThree obj)
        {
            obj.Created = DateTime.UtcNow;
            return base.PostAsync(obj);
        }
    }
}
