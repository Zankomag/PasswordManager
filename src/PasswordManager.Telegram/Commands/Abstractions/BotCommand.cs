using PasswordManager.Telegram.Services.Abstractions;

namespace PasswordManager.Telegram.Commands.Abstractions; 

public abstract class BotCommand : IBotCommand {

	//todo check if name should start with lowercase
	protected readonly IBot Bot;

	protected BotCommand(IBot bot) => this.Bot = bot;

}