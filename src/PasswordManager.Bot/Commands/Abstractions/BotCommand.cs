using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands.Abstractions {
	public abstract class BotCommand : IBotCommand {
		protected readonly IBot botService;

		public BotCommand(IBot botService) {
			this.botService = botService;
		}

	}
}
