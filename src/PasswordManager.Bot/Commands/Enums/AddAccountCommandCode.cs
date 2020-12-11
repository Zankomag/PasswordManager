﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Bot.Commands.Enums {
	/// <summary>
	/// CallbackQuery command code for <see cref="PasswordManager.Bot.Commands.AddAccountCommand"/>
	/// </summary>
	public enum AddAccountCommandCode {
		SkipLink = 'S',
		AutoLink = 'A',
		SkipEncryptionyonKey = 'E',
		AcceptPassword = 'Z',
	}
}
