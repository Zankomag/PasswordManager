
namespace UPwdBot {
	interface ICommand {
		string Name { get; set; }
		void Execute();
	}
}
