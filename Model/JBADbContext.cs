using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JBA.Model
{
    class JBADbContext : DbContext
    {
        public DbSet<PrecipitationRecord> PrecipitationRecords { get; set; }

        private string DatabaseName;

        public JBADbContext(string name)
        {
            DatabaseName = name;
            Directory.CreateDirectory("output");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=output\\{DatabaseName}.db");
        }
    }
}
