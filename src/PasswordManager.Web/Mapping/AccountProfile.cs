using AutoMapper;
using PasswordManager.Core.Entities;

namespace PasswordManager.Web.Mapping; 

public class AccountProfile : Profile {
	public AccountProfile() {
		CreateMap<Account, Account>();
	}
}