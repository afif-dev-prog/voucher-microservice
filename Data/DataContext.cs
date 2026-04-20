using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using voucherMicroservice.Model;

namespace voucherMicroservice.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Floating> floating { get; set; }
        public DbSet<PayHistory> payhistory { get; set; }
        public DbSet<Seller> seller { get; set; }
        public DbSet<StaffList> stafflist { get; set; }
        public DbSet<Student> student { get; set; }
        public DbSet<AuthLog> AuthLog { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<TokenBlacklist> TokenBlacklist { get; set; }
        public DbSet<SellerBalanceSnapshot> sellerBalanceSnapshot { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tell EF Core that UUID is generated on insert — not by DB default
            modelBuilder.Entity<AuthLog>()
                .Property(e => e.id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Permission>()
                .Property(e => e.id)
                .ValueGeneratedNever();

            modelBuilder.Entity<RolePermission>()
                .Property(e => e.id)
                .ValueGeneratedNever();

            modelBuilder.Entity<UserPermission>()
                .Property(e => e.id)
                .ValueGeneratedNever();

            modelBuilder.Entity<TokenBlacklist>()
                .Property(e => e.id)
                .ValueGeneratedNever();
        }
    }



    // public class DataContextBackup : DbContext
    // {
    //     public DataContextBackup(DbContextOptions<DataContextBackup> options) : base(options)
    //     {

    //     }
    //     public DbSet<Floating> floating { get; set; }
    //     public DbSet<PayHistory> payhistory { get; set; }
    //     public DbSet<Seller> seller { get; set; }
    //     public DbSet<StaffList> stafflist { get; set; }
    //     public DbSet<Student> student { get; set; }
    // }

}