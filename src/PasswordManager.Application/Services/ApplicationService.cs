using PasswordManager.Application.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace PasswordManager.Application.Services; 

//TODO i thing we need to get rid of this and just use options as in other pfojects
public class ApplicationService : IApplicationService {
	public long[] AdminIds { get; }

	public ApplicationService(IOptions<ApplicationSettings> appSettingsConfig) {
		ApplicationSettings appSettings = appSettingsConfig.Value;
		AdminIds = appSettings.AdminIds;
	}
}