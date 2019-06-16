using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace UPwdBot {
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
