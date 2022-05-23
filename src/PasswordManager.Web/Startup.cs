using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PasswordManager.Application.Services;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Commands;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services;
using PasswordManager.Core.Repositories;
using PasswordManager.Infrastructure.Data;
using Serilog;
using System.Linq;
using System.Reflection;
using PasswordManager.Application;
using AutoMapper;
using PasswordManager.Bot.Settings;
using PasswordManager.Infrastructure.Extensions;
using PasswordManager.Infrastructure.Repositories;

namespace PasswordManager.Web; 

public class Startup {
	public Startup(IConfiguration configuration) {
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	public void ConfigureServices(IServiceCollection services) {

		services.Configure<BotSettings>(Configuration.GetSection(nameof(BotSettings)));

		services.AddSqlServer(Configuration);

		//TODO MOVE ALL INITIALIZATION FROM OTHER PROJECTS TO THERS ServiceCollectionExtentions methods

		#region Telegram Bot
		//
		//TODO move all this DI in infrastructure layer or corresponding layer and call injection medods from infrastructure
		//but here only one infrastructure method
		//

		//TODO:
		//make that botsettings will be configured so that that have AdminIds from "ApplicationSettings" section
		
		services.AddSingleton<IBot, Bot.Services.Bot>();
		services.AddSingleton<ICommandFactory, CommandFactory>();
		services.AddSingleton<IAccountUpdatingService, AccountUpdatingService>();
		services.AddSingleton<IAccountAssemblingService, AccountAssemblingService>();
		services.AddSingleton<IPasswordDecryptionService, PasswordDecryptionService>();
		services.AddSingleton<IPasswordEncryptionService, PasswordEncryptionService>();

		services.AddScoped<IBotHandler, BotHandler>();
		services.AddScoped<IBotUi, BotUi>();
		services.AddScoped<IBotUserService, BotUserService>();
			

		//Add all commands using reflection
		IEnumerable<Type> botCommands = Assembly.GetAssembly(typeof(IBotCommand))
			?.GetExportedTypes()
			.Where(x => x.IsAssignableFrom(typeof(IBotCommand)) && x.IsClass && !x.IsAbstract);
		foreach (var commandType in botCommands) {
			services.AddScoped(commandType);
		}
		#endregion
		#region Application
		services.AddSingleton<IApplicationService, ApplicationService>();

		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IAccountService, AccountService>();
		#endregion
		#region Infrastructure
		services.AddScoped<IUnitOfWork, UnitOfWork>();
		#endregion

		services.AddAutoMapper(typeof(Startup));

		services.AddControllers()
			.AddNewtonsoftJson(options => {
				options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			});
		
		//todo check on this:
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


		app.UseEndpoints(endpoints => endpoints.MapControllers());
	}
}