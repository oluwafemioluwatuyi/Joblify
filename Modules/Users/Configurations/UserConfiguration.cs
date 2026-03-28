using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Joblify.Modules.Users.Entities;

namespace Joblify.Modules.Users.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Description)
               .HasMaxLength(2000);

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
               .IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        // TODO: Add relationships, additional indexes, or constraints here
    }
}
