
using System.Threading;
using System.Threading.Tasks;
#if EF6
using System.Data.Entity;
using System.Reflection;
#elif EFCore
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endif
using Numero3.EntityFramework.Demo.DomainModel;
using EntityFramework.DbContextScope.Interfaces;

namespace Numero3.EntityFramework.Demo.DatabaseContext
{
	public class UserManagementDbContext : DbContext, IDbContext
	{
		// Map our 'User' model by convention
		public DbSet<User> Users { get; set; }
#if EF6
        public UserManagementDbContext() : base("Server=localhost;Database=DbContextScopeDemo;Trusted_Connection=true;")
		{}

	    protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Overrides for the convention-based mappings.
			// We're assuming that all our fluent mappings are declared in this assembly.
			modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(UserManagementDbContext)));
		}
#elif EFCore
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=DbContextScopeDemo;Trusted_Connection=true;");
        }

	    protected override void OnModelCreating(ModelBuilder modelBuilder)
	    {
	        EntityTypeBuilder<User> entityTypeBuilder = modelBuilder.Entity<User>();
	        entityTypeBuilder.Property(user => user.Name).IsRequired();
	        entityTypeBuilder.Property(user => user.Email).IsRequired();
	    }
#endif
	    
	}
}
