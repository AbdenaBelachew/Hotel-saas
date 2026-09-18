using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(p => p.PropertyType).HasColumnName("property_type").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.Address).HasColumnName("address");
        builder.Property(p => p.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(p => p.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(p => p.Website).HasColumnName("website").HasMaxLength(250);
        builder.Property(p => p.LogoUrl).HasColumnName("logo_url");
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        builder.Property(p => p.Timezone).HasColumnName("timezone").HasMaxLength(100).IsRequired();
        builder.Property(p => p.CheckInTime).HasColumnName("check_in_time").IsRequired();
        builder.Property(p => p.CheckOutTime).HasColumnName("check_out_time").IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(30).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(p => new { p.OrganizationId, p.Code }).IsUnique();
        builder.HasIndex(p => p.OrganizationId);
    }
}
