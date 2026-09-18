using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.OrganizationId).HasColumnName("organization_id");
        builder.Property(a => a.PropertyId).HasColumnName("property_id");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasColumnName("entity_name").HasMaxLength(150);
        builder.Property(a => a.EntityId).HasColumnName("entity_id");
        builder.Property(a => a.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        // ip_address is a native Postgres `inet` column; not mapped yet since Phase 1 never
        // populates it (no CLR type has been wired to that conversion). Revisit once request
        // context (client IP) is threaded through the audit interceptor.
        builder.Ignore(a => a.IpAddress);
        builder.Property(a => a.UserAgent).HasColumnName("user_agent");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(a => a.OrganizationId);
        builder.HasIndex(a => a.PropertyId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.CreatedAt);
    }
}
