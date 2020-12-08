using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Newtonsoft.Json;
using PasswordManager.Infrastructure.Data;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands;
using PasswordManager.Core.Repositories;
using PasswordManager.Infrastructure.Repository;
using System.Reflection;
using System.Linq;

namespace PasswordManager.Bot {

	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {

			services.Configure<BotSettings>(Configuration.GetSection(nameof(BotSettings)));
			
			services.AddDbContext<PasswordManagerDbContext>(options => {
				options.UseSqlite(Configuration.GetConnectionString("PasswordManager"));
				////Adding "Microsoft.EntityFrameworkCore": "Information" 
				////to Serilog MinimumLevel in config  allows to get more convenient output
				//options.LogTo(System.Console.WriteLine, minimumLevel: LogLevel.Information);
			});

#region Telegram Bot
			services.AddSingleton<IBotService, BotService>();
			services.AddSingleton<ICommandFactory, CommandFactory>();
			services.AddScoped<IBotHandler, BotHandler>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			var botCommands = Assembly.GetAssembly(typeof(IBotCommand))
				.GetExportedTypes()
				.Where(x => x.IsAssignableFrom(typeof(IBotCommand)) && x.IsClass);
			//Because IEnumerable doesn't have ForEach()
			foreach(var commandType in botCommands) {
				services.AddScoped(commandType);
			}
#endregion


			services.AddControllers()
				.AddNewtonsoftJson(options => {
					options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
					options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
				});
				//.ConfigureApiBehaviorOptions(options => {
				//	//Override default model state error response
				//	options.InvalidModelStateResponseFactory = context => {
				//		if (context.ModelState.ErrorCount > 0) {
				//			string messages = string.Join("; ", context.ModelState.Values
				//				.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
				//			return (ObjectResult)new Response<object>(messages);
				//		}
				//		return (ObjectResult)Response<object>.BadRequestResposne;
				//	};
				//});
		}

		public void Configure(IApplicationBuilder app) {

			//To log only Warning or greater requests
			//Set "Serilog.AspNetCore": "Warning" in Serilog MinimumLevel Config
			app.UseSerilogRequestLogging();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
