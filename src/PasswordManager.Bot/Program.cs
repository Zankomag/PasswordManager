using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PasswordManager.Bot {
	public class Program {
		public static void Main() {
			Bot.Instance.ReportStart().Wait();
			CreateWebHostBuilder().Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder() =>
			WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>();
	}
}
