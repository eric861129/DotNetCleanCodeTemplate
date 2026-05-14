using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Orders;

namespace Template.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.CustomerId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.PaidAt);

        builder.OwnsMany(order => order.Items, item =>
        {
            item.ToTable("OrderItems");
            item.WithOwner().HasForeignKey("OrderId");
            item.HasKey(nameof(OrderItem.Id));

            item.Property(orderItem => orderItem.Id).ValueGeneratedNever();
            item.Property(orderItem => orderItem.ProductName)
                .IsRequired()
                .HasMaxLength(200);
            item.Property(orderItem => orderItem.Quantity).IsRequired();
            item.Property(orderItem => orderItem.UnitPrice).HasPrecision(18, 2);
            item.Ignore(orderItem => orderItem.LineTotal);
        });

        builder.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
