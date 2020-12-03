using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;

namespace PasswordManager.Infrastructure.Data {
	public class PasswordManagerDbContext : DbContext {

		public PasswordManagerDbContext() : base() { }

		public PasswordManagerDbContext(DbContextOptions options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Account> Accounts { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseSqlite("DataSource = DB\\pwd.db");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			//Disable User.Id Autoincrement, because User.Id = Telegram User Id
			modelBuilder.Entity<User>().Property(u => u.Id)
				.ValueGeneratedNever();
		}
	}
}
