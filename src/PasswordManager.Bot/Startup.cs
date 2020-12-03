using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Bot {

	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {

			//TODO
			//REPLACE WITH Configuration.GetConnectionString
			string connection = "Data Source = DB\\pwd.db"; //Configuration.GetConnectionString("PasswordManagerConnectionString");
			services.AddDbContext<PasswordManagerDbContext>(options => {
				options.UseSqlite(connection);
				//Adding "Microsoft.EntityFrameworkCore": "Information" 
				//to Serilog MinimumLevel in config  allows to get more convenient output
				//options.LogTo(System.Console.WriteLine, minimumLevel: LogLevel.Information);
			});


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

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}
	}
}
