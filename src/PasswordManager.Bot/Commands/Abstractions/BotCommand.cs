using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands.Abstractions; 

public abstract class BotCommand : IBotCommand {

	//todo check if name should start with lowercase
	protected readonly IBot Bot;

	protected BotCommand(IBot bot) => this.Bot = bot;

}