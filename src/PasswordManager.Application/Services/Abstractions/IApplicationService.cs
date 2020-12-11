
namespace PasswordManager.Application.Services.Abstractions {
	public interface IApplicationService {
		/// <summary>
		/// List of Admin Ids
		/// </summary>
		int[] Admins { get; }
	}
}
