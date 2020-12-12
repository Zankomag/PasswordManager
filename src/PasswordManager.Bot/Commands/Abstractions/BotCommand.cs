using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands.Abstractions {
	public abstract class BotCommand : IBotCommand {
		protected readonly IBot bot;

		public BotCommand(IBot bot) {
			this.bot = bot;
		}

	}
}
