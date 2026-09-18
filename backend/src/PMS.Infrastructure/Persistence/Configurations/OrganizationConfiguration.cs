using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.LegalName).HasColumnName("legal_name").HasMaxLength(250);
        builder.Property(o => o.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(o => o.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(o => o.Phone).HasColumnName("phone").HasMaxLength(50);
        builder.Property(o => o.Address).HasColumnName("address");
        builder.Property(o => o.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(o => o.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(o => o.LogoUrl).HasColumnName("logo_url");
        builder.Property(o => o.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        builder.Property(o => o.Timezone).HasColumnName("timezone").HasMaxLength(100).IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasMaxLength(30).HasConversion<string>().IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(o => o.Code).IsUnique();

        builder.HasMany(o => o.Properties)
            .WithOne(p => p.Organization)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
