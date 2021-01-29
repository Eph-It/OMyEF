using System;
using Microsoft.EntityFrameworkCore;
using OMyEF.Db;
using System.ComponentModel.DataAnnotations;

namespace OMyEFDbContext
{
    [GenerateODataController]
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
    [GenerateODataController]
    public class TableThree
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
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

        public virtual DbSet<TableOne> TableOne { get; set; }
        public virtual DbSet<TableTwo> TableTwo { get; set; }
        public virtual DbSet<TableThree> TableThree { get; set; }

    }
}
