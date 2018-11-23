using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TestMVC.Models
{
    public class AppBoxMvcContext : DbContext
    {
        // 如果需要使用 Web.config 中的数据库连接字符串，请修改为 base("Default")，其中 Default 为数据库连接字符串的名称
        public AppBoxMvcContext()
            : base("Default") 
        {
        }

        public DbSet<Config> Configs { get; set; }
        public DbSet<Dept> Depts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Online> Onlines { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Power> Powers { get; set; }
        public DbSet<Menu> Menus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithMany(u => u.Roles)
                .Map(x => x.ToTable("RoleUsers")
                    .MapLeftKey("RoleID")
                    .MapRightKey("UserID"));

            modelBuilder.Entity<Title>()
                .HasMany(t => t.Users)
                .WithMany(u => u.Titles)
                .Map(x => x.ToTable("TitleUsers")
                    .MapLeftKey("TitleID")
                    .MapRightKey("UserID"));

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Powers)
                .WithMany(p => p.Roles)
                .Map(x => x.ToTable("RolePowers")
                    .MapLeftKey("RoleID")
                    .MapRightKey("PowerID"));


            // 注意 Map 和 HasForeignKey 的区别
            modelBuilder.Entity<Dept>()
                .HasOptional(d => d.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(d => d.ParentID);

            modelBuilder.Entity<Menu>()
                .HasOptional(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(d => d.ParentID);





            modelBuilder.Entity<Dept>()
                .HasMany(d => d.Users)
                .WithOptional(d => d.Dept)
                .HasForeignKey(d => d.DeptID);

            modelBuilder.Entity<Online>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(d => d.UserID);

            modelBuilder.Entity<Menu>()
                .HasOptional(m => m.ViewPower)
                .WithMany()
                .HasForeignKey(d => d.ViewPowerID);

        }
    }
}