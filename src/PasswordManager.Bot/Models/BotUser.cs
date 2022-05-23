using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Models; 

public class BotUser {
		
	/// <summary>
	/// This is the same Id as <see cref="User"/>.<see cref="User.Id"/>
	/// </summary>
	public long Id { get; init; }
		
	/// <summary>
	/// Language code
	/// </summary>
	public string Lang { get; set; }
	public UserAction Action { get; set; }


	public static implicit operator BotUser(User user) {
		if (user == null) 
			return null;
		return new BotUser {
			Id = user.Id,
			Lang = user.Language,
			Action = user.Action
		};
	}
}