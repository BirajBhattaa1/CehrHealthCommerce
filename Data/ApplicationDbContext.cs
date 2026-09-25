using NepalMediHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Data;

/// <summary>
/// EF Core database context. Inherits Identity tables and adds the domain model.
/// All data access goes through EF Core (parameterized) — no raw SQL.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- ApplicationUser ---
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(120).IsRequired();
            e.Property(u => u.NID).HasMaxLength(40).IsRequired();
            e.HasIndex(u => u.NID).IsUnique();

            e.HasOne(u => u.Cart)
             .WithOne(c => c.User!)
             .HasForeignKey<Cart>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Category ---
        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();

            e.HasOne(c => c.ParentCategory)
             .WithMany(c => c.Children)
             .HasForeignKey(c => c.ParentCategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Product ---
        builder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.SKU).IsUnique();

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Cart / CartItem ---
        builder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.Cart)
             .WithMany(c => c.Items)
             .HasForeignKey(ci => ci.CartId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ci => ci.Product)
             .WithMany(p => p.CartItems)
             .HasForeignKey(ci => ci.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Order / OrderItem ---
        builder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.OrderStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);

            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Payment (1:1 with Order) ---
        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(p => p.OrderId).IsUnique();
            e.HasIndex(p => p.TransactionUuid).IsUnique();

            e.HasOne(p => p.Order)
             .WithOne(o => o.Payment!)
             .HasForeignKey<Payment>(p => p.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Address ---
        builder.Entity<Address>(e =>
        {
            e.HasOne(a => a.User)
             .WithMany(u => u.Addresses)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Prescription ---
        builder.Entity<Prescription>(e =>
        {
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(p => p.User)
             .WithMany()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Order)
             .WithMany()
             .HasForeignKey(p => p.OrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
