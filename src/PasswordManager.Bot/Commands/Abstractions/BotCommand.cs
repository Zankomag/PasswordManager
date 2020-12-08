using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Commands.Abstractions {
	public abstract class BotCommand : IBotCommand {
		protected readonly IBotService botService;

		public BotCommand(IBotService botService) {
			this.botService = botService;
		}

	}
}
