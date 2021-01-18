using System;
using Microsoft.EntityFrameworkCore;
using OMyEF;
using System.ComponentModel.DataAnnotations;

namespace OMyEFDbContext
{
    public class TableOne{
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class TableTwo{
        [Key]
        public int Id {get;set;}
        public string Name {get;set;}
        public string Description {get;set;}
    }
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


//dotnet add package Microsoft.EntityFrameworkCore.InMemory