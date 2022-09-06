using Junetic.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Telegram.Services;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Telegram.Settings;

// ReSharper disable UnusedMethodReturnValue.Global

namespace PasswordManager.Telegram.Extensions;

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<BotSettings>(configuration, BotSettings.SectionName);
		services.AddSingleton<IBotService, BotService>();
		services.AddSingleton<ITelegramBotUi, TelegramBotUi>();
		return services;
	}

}