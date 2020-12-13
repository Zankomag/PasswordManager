using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IBotUserService {
		/// <summary>
		/// Retrieves Telegram user id from update and returns corresponding user.
		/// If user cannot or isn't allowed to be registered, returns null
		/// </summary>
		Task<BotUser> GetUser(Update update);
	}
}
