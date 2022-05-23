using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

// ReSharper disable ConvertToLambdaExpressionWhenPossible

namespace PasswordManager.Infrastructure.Extensions; 

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration) {
		services.AddDbContext<PasswordManagerDbContext>(options => {
			//options.UseSqlite(Configuration.GetConnectionString("PasswordManager"));
			//todo use constant name from settings
			options.UseSqlServer(configuration.GetConnectionString("PasswordManager"));

			//, builder => builder.MigrationsAssembly("PasswordManager.Web"));
			////Adding "Microsoft.EntityFrameworkCore": "Information" 
			////to Serilog MinimumLevel in config  allows to get more convenient output
			//options.LogTo(System.Console.WriteLine, minimumLevel: LogLevel.Information);
		});
		return services;

	}

}