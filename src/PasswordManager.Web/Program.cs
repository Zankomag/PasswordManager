using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PasswordManager.Telegram;
using Serilog;

//todo move to dotnet 6 and use file-scoped usings
namespace PasswordManager.Web; 

public class Program {
	public static void Main(string[] args) {
		//TODO use appsettings.ENV.json too
		//todo MAKE NORMAL INITIALIZATION OF CONFIGURATION, NOT LIKE THAT!!!, 
		// and inject Logger after that
		var config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		//Log.Logger = new LoggerConfiguration()
		//	.ReadFrom.Configuration(config)
		//	.CreateLogger();
//
		//Log.Information("Application starting");
		
		//deprecated project
		//CreateHostBuilder(args).Build().Run();
	}

	// public static IHostBuilder CreateHostBuilder(string[] args) =>
	// 	Host.CreateDefaultBuilder(args)
	// 		//.UseSerilog()
	// 		.ConfigureWebHostDefaults(webBuilder => {
	// 			webBuilder.UseStartup<Startup>();
	// 		});
}