using Microsoft.EntityFrameworkCore;
using PasswordManager.Database;
using System.Configuration;

namespace Server
{
    public class PasswordManager_db : DbContext
    {
        public PasswordManager_db() 
        {

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
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CS_AS");
            modelBuilder.Entity<Autorization_data>()
                .HasOne(a => a.Account)
                .WithMany(a => a.Autorization_Data)
                .HasForeignKey(a => a.AccountId);
        }
    }
}
