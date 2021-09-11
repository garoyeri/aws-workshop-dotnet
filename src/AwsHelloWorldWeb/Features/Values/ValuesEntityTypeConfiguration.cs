namespace AwsHelloWorldWeb.Features.Values
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ValuesEntityTypeConfiguration : IEntityTypeConfiguration<DatabaseValueItem>
    {
        public void Configure(EntityTypeBuilder<DatabaseValueItem> builder)
        {
            builder.ToTable("Values").HasKey(v => v.Id);
            builder.Property(v => v.Value).IsRequired();
        }
    }
}