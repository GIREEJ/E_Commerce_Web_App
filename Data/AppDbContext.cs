using ECommerceWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ECommerceWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Force all DateTime properties to use "timestamp without time zone"
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProps = entityType.ClrType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var prop in dateTimeProps)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(prop.Name)
                        .HasColumnType("timestamp without time zone");
                }
            }

            // User -> Country
            modelBuilder.Entity<User>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> State
            modelBuilder.Entity<User>()
                .HasOne(u => u.State)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.StateId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> City
            modelBuilder.Entity<User>()
                .HasOne(u => u.City)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Country -> State
            modelBuilder.Entity<Country>()
                .HasMany(c => c.States)
                .WithOne(s => s.Country)
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // State -> City
            modelBuilder.Entity<State>()
                .HasMany(s => s.Cities)
                .WithOne(c => c.State)
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product Price Precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            // Order TotalAmount Precision
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18, 2)");

            // OrderItem UnitPrice Precision
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18, 2)");

            // Seed Admin Data
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    AdminId = 1,
                    FullName = "Super Admin",
                    Email = "superadmin@ecommerce.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEKvQaqA6AvZkxdZuAcUG1GiqX+44p0MRDh6kMgQbd5hZzB3LFd+BDZGNV1zKqU9Z/g==",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0)
                });
        }
    }
}
