using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services; 

public class BotUserService : IBotUserService {
	private readonly IBot bot;
	private readonly IUserService userService;
	private readonly ICommandFactory commandFactory;

	public BotUserService(IBot bot, IUserService userService, ICommandFactory commandFactory) {
		this.bot = bot;
		this.userService = userService;
		this.commandFactory = commandFactory;
	}

	//todo check if we don't have Int32 userid anywhere in code
	public virtual async Task<BotUser> GetUser(Update update) {
		long? userId;
		if ((userId = update.GetUserIdByUpdateType()) == null)
			return null;

		BotUser botUser = await userService.GetUserAsync(userId.Value);
		if (botUser == null) {
			//todo move this block behaviour to overridable method
			//If bot is private, only admins can be treated as new users
			//In case user is not admin bot doesn't respond with message
			//explaining that user is not registered, instead bot 
			//ignores user pretending it's dead.
			//This behavior can vary depending on your requirements
			if (!bot.IsPublic && !bot.IsUserAdmin(userId.Value))
				return null;
			return await RegisterUser(update);
		}
		return botUser;
	}

	/// <summary>
	/// Register new user and returns it.
	/// Returns null if unable to register user
	/// </summary>
	/// <param name="update"></param>
	/// <returns></returns>
	private async Task<BotUser> RegisterUser(Update update) {
		BotUser botUser = null;
		//User must choose language before be registered and using any command
		if (update.Type == UpdateType.Message) {
			if (Localization.ContainsLanguage(update.Message.From.LanguageCode)) {
				botUser = await userService.AddUserAsync(
					update.Message.From.Id, update.Message.From.LanguageCode);
			} else {
				//TODO: 
				//Use key from new BotCommand system, not hardcoded
				//
				//User is not registered here as in CallbackQuery Handler
				//because he should not be treated as registered user
				//until he selects language. So he is invited to select one.
				await commandFactory.GetMessageCommand("/language")
					.ExecuteAsync(update.Message, new BotUser {
						Id = update.Message.From.Id,
						Lang = Localization.DefaultLanguageCode
					});
			}
		} else if (update.Type == UpdateType.CallbackQuery) {
			//Register new user when he selected language for the first time
			if (update.CallbackQuery.Data[0] == (char)CallbackQueryCommandCode.SelectLanguage) {
				botUser = await userService
					.AddUserAsync(update.CallbackQuery.From.Id, Localization.DefaultLanguageCode);
				await commandFactory.GetCallBackQueryCommand(CallbackQueryCommandCode.SelectLanguage)
					.ExecuteAsync(update.CallbackQuery, botUser);
			}
		}
		return botUser;
	}
}