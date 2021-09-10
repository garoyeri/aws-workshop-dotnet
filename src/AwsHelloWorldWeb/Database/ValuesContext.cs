namespace AwsHelloWorldWeb.Database
{
    using Microsoft.EntityFrameworkCore;

    public class ValuesContext : DbContext
    {
        public ValuesContext(DbContextOptions<ValuesContext> options) : base(options)
        {
        }
    }
}