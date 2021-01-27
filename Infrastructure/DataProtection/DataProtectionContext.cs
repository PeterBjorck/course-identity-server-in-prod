using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataProtection
{
    public class DataProtectionContext : DbContext, IDataProtectionKeyContext
    {
        public DataProtectionContext(DbContextOptions<DataProtectionContext> options)
            : base(options) { }

        // This maps to the table that stores keys.
        public DbSet<DataProtectionKey>? DataProtectionKeys { get; set; } = null!;
    }
}
