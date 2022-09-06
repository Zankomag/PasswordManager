using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Telegram.Commands.Enums; 

/// <summary>
/// CallbackQuery command code for <see cref="AddAccountCommand"/>
/// </summary>
public enum AddAccountCommandCode {
	SkipLink = 'S',
	SkipNote = 'N',
	AutoLink = 'A',
	SkipEncryptionKey = 'E',
	AcceptPassword = 'Z',
}