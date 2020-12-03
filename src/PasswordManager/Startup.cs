using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace PasswordManager {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {

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
