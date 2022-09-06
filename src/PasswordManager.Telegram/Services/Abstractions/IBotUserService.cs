using System.Threading.Tasks;
using PasswordManager.Telegram.Models;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Services.Abstractions; 

public interface IBotUserService {
	/// <summary>
	/// Retrieves Telegram user id from update and returns corresponding user.
	/// If user cannot or isn't allowed to be registered, returns null
	/// </summary>
	Task<BotUser> GetUser(Update update);
}