

// ReSharper disable UnusedMethodReturnValue.Global

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PasswordManager.Common.Utilities;

namespace PasswordManager.Common.Extensions;

//todo move to library Junetic.AspNetCore.Common
public static class ServiceCollectionExtensions {

	public static IServiceCollection AddOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName) where TOptions : class {
		var configSection = configuration.GetSection(sectionName);
		services.Configure<TOptions>(configSection);
		services.AddOptionsValidator<TOptions>();
		return services;
	}

	public static IServiceCollection AddOptionsValidator<TOptions>(this IServiceCollection services) where TOptions : class {
		services.AddSingleton<IValidateOptions<TOptions>, OptionsValidator<TOptions>>();
		return services;
	}

}