using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PasswordManager_db : DbContext
    {
        public PasswordManager_db() 
        { 
            //Database.EnsureCreated();
            //Database.EnsureDeleted();
        }
        public DbSet<Autorization_data> Autorization_Data { get; set; }
        public DbSet<Account> Accounts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["PasswordManager_db"].ConnectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Autorization_data>()
                .HasOne(a => a.Account)
                .WithMany(a => a.Autorization_Data)
                .HasForeignKey(a => a.AccountId);
        }
    }
}
