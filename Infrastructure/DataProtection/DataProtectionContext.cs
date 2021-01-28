using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataProtection
{
    //Disable the nummable warning in this file
#nullable disable

    /// <summary>
    /// The source code to the EntityFrameworkCoreXmlRepository can be found here:
    /// https://github.com/dotnet/aspnetcore/blob/master/src/DataProtection/EntityFrameworkCore/src/EntityFrameworkCoreXmlRepository.cs
    /// </summary>
    public class ClientDataProtectionContext : DbContext, IDataProtectionKeyContext
    {
        public ClientDataProtectionContext(DbContextOptions<ClientDataProtectionContext> options)
            : base(options) { }

        // This maps to the table that stores keys.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys_Client");
        }
    }

    public class IdentityServiceDataProtectionContext : DbContext, IDataProtectionKeyContext
    {
        public IdentityServiceDataProtectionContext(DbContextOptions<IdentityServiceDataProtectionContext> options)
            : base(options) { }

        // This maps to the table that stores keys.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys_IdentityService");
        }
    }


    public class PaymentApiDataProtectionContext : DbContext, IDataProtectionKeyContext
    {
        public PaymentApiDataProtectionContext(DbContextOptions<PaymentApiDataProtectionContext> options)
            : base(options) { }

        // This maps to the table that stores keys.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys_PaymentAPI");
        }
    }


}
