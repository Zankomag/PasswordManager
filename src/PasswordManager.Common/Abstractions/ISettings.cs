namespace PasswordManager.Common.Abstractions;

/// <summary>
/// Used for configuration class for usage with Options pattern,
/// getting configuration from appsettings.json files
/// </summary>
public interface ISettings {

	/// <summary>
	/// The name of configuration section in appsettings.json file
	/// </summary>
	public string SectionName { get; }

}