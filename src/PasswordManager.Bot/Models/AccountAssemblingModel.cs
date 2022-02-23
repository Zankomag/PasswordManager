using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PasswordManager.Bot.Services.Enums;

namespace PasswordManager.Bot.Models {
	//TODO: Move validation to Service
	public class AccountAssemblingModel {
		public long Id { get; set; }

		public long UserId { get; set; }

		private string accountName;
		public string AccountName { get => accountName;
			set {
				var account = new Account { AccountName = value };
				Validate(account.AccountName, account, nameof(account.AccountName));
				accountName = value;
			} }

		private string link;
		public string Link {
			get => link;
			set {
				var account = new Account { Link = value };
				Validate(account.Link, account, nameof(account.Link));
				link = value;
			}
		}

		private string login;
		public string Login {
			get => login;
			set {
				var account = new Account { Login = value };
				Validate(account.Login, account, nameof(account.Login));
				login = value;
			}
		}

		private string password;
		public string Password {
			get => password;
			set {
				var account = new Account { Password = value };
				Validate(account.Password, account, nameof(account.Password));
				password = value;
			}
		}

		private string note;
		public string Note {
			get => note;
			set {
				var account = new Account { Note = value };
				Validate(account.Note, account, nameof(account.Note));
				note = value;
			}
		}

		public bool Encrypted { get; set; }



		public AccountAssemblingStage AccountAssemblingStage { get; set; }

		//TODO:
		//DEbug this
		//
		//Value must be property of instance
		//memberName must be namof(instance.Property)
		private void Validate(object value, Account instance, string memberName) {
			ICollection<ValidationResult> validationResults = null;
			if (!Validator.TryValidateProperty(value, new ValidationContext(instance) {MemberName = memberName }, validationResults)) {
				throw new ValidationException(validationResults?.FirstOrDefault().ErrorMessage);
			}
		}
	}
}
