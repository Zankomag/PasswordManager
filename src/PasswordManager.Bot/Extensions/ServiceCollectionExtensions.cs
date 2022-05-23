﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Bot.Services;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Settings;
using PasswordManager.Common.Extensions;

// ReSharper disable UnusedMethodReturnValue.Global

namespace PasswordManager.Bot.Extensions;

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddTelegramBotServices(this IServiceCollection services, IConfiguration configuration) {
		services.AddOptions<BotSettings>(configuration, BotSettings.SectionName);
		services.AddSingleton<IBotService, BotService>();
		services.AddSingleton<IBotUi, BotUi>();
		return services;
	}

}