using System.Threading.Tasks;
using Junetic.Common.Extensions;
using Junetic.Common.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager.Telegram.Services;

namespace PasswordManager.Telegram;

public class LocalEntryPoint {

	private static async Task Main() {
		IHost host;
		if(EnvironmentWrapper.IsDevelopment) {
			host = GetHost();
		} else {
			host = GetWebHost();
		}

		await host.RunAsync();
	}

	private static IHost GetHost()
		=> new HostBuilder()
			.AddConfiguration<LocalEntryPoint>()
			.UseStartup<Startup>()
			.ConfigureServices(services => services.AddHostedService<TelegramBotLocalRunner>())
			.Build();

	private static IHost GetWebHost()
		=> new HostBuilder()
			.AddConfiguration<LocalEntryPoint>()
			.ConfigureWebHost(x =>
				x.UseKestrel((builderContext, options) => options.Configure(builderContext.Configuration.GetSection("Kestrel")))
					.UseStartup<Startup>())
			.Build();

}