using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace UPwdBot {
	public class Program {
		public static void Main() {
			//BotHandler.Init(); //method renamed to static constructor
			//Localization.Initialize(); //method renamed to static constructor
			Bot.Instance.ReportStart().Wait();
			CreateWebHostBuilder().Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder() =>
			WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>();
	}
}
