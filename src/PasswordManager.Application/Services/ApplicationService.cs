using PasswordManager.Application.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace PasswordManager.Application.Services {
	public class ApplicationService : IApplicationService {
		public int[] Admins { get; }

		public ApplicationService(IOptions<ApplicationSettings> appSettingsConfig) {
			ApplicationSettings appSettings = appSettingsConfig.Value;
			Admins = appSettings.AdminIds;
		}
	}
}
