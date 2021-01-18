# OMyEF

A source code generator to manage Entity Framework tables through OData. 

Easily add OData controllers for any DbSet with just one attribute to manage the table!

## Install Instructions

```
dotnet add package OMyEF
```

## Usage

Add OMyEF package to the project holding your DbContext and WebAPI (sometimes these are the same, sometimes not). 

After adding it to these projects, in your DbContext where DbSets are defined, generate OData APIs by adding the ```GenerateODataController``` attribute to DbSets:

``` C#
using System;
using Microsoft.EntityFrameworkCore;
using OMyEF;

namespace OMyEFDbContext
{
    public class MyDbContext : DbContext
    {
        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }
        [GenerateODataController]
        public virtual DbSet<TableOne> TableOne { get; set; }
        public virtual DbSet<TableTwo> TableTwo { get; set; }

    }
}

```

In the above example, we'll generate an OData controller for TableOne but not TableTwo.

Then in  your web server project, in ConfigureServices ensure your DbContext is added and run the ```AddOMyEF()``` method:

``` C#
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddDbContext<MyDbContext>(options => options.UseInMemoryDatabase("OMyDb"));
            services.AddOMyEF();
        }
```

Lastly, in ```Startup.cs``` again under Configure, add the routing like this:

``` C#
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseEndpoints(endpoints =>
            {
                // Any other endpoints can be here
                endpoints.AddOMyEFRoute<MyDbContext>();
            });
        }
```

If you want a complete example - there's an [Example Projects](https://github.com/Eph-It/OMyEF/tree/main/src/ExampleProjects) that has a basic DbContext and standard WebAPI that has these added.

Once set up, you can run your WebAPI and start querying your data at ```<webApiURI>/odata/TableName```. View the OData metadata at ```<webApiURI>/odata/metadata$```.

## Road map

### Version: 0.1
- [x] Can query database

### Version: 0.2
- [ ] Can perform edits on tables

### Version: 0.3
- [ ] Use dependency injection and an interface to support BeforeEdit & AfterEdit extensions (ie, BeforeEdit to modify the data, like add a DateModified and AfterEdit to perform more data)

### Version: 0.4
- [ ] Add CanEdit / CanDelete / CanCreate methods to DI to support more advanced security scenarios

### Version: 0.5
- [ ] Support changing odata route
- [ ] Support more advanced routing scenarios