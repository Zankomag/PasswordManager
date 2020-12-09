using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Bot.Abstractions {
	public interface IBotUIService {
		/// <summary>
		/// Sends account data with buttons
		/// </summary>
		/// <param name="messageToEditId">If specified, message will be edited instead of sending new</param>
		/// <param name="extraMessage">An Extra message to show with account data</param>
		/// <returns></returns>
		Task ShowAccount(BotUser user, Account account, int? messageToEditId = null, string extraMessage = null);
	}
}
