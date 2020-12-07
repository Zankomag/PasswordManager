using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Abstractions {
	public interface IBotHandlerService {
		void HandleUpdate(Update update);
	}
}
