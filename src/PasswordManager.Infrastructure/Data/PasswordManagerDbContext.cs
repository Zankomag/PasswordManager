using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using System;

namespace PasswordManager.Infrastructure.Data; 

public class PasswordManagerDbContext : DbContext {

	public PasswordManagerDbContext() { }

	public PasswordManagerDbContext(DbContextOptions options) : base(options) { }

	public DbSet<User> Users { get; set; }
	public DbSet<Account> Accounts { get; set; }

	//todo remove this
	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
	//	optionsBuilder.UseSqlite("Data Source=DB\\pwd.db");
	//}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		//Disable User.Id Autoincrement, because User.Id = Telegram User Id
		modelBuilder.Entity<User>().Property(u => u.Id)
			.ValueGeneratedNever();

		//Set default value for 6 month
		modelBuilder.Entity<User>().Property(u => u.OutdatedTime)
			.HasDefaultValue(new TimeSpan(days: 365, 0, 0, 0));

		modelBuilder.Entity<User>()
			.Property(e => e.PasswordGeneratorPattern).HasColumnName("PasswordGeneratorPattern");
	}
}