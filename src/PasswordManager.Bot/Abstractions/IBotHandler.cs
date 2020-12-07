using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Abstractions {
	public interface IBotHandler {
		void HandleUpdate(Update update);
	}
}
