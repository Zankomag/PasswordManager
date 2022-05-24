

using Junetic.Common.Abstractions;

namespace PasswordManager.Application.Settings; 

public class ApplicationSettings : ISettings {
	
	/// <inheritdoc/>
	public static string SectionName => "Application";
	
	public long[] AdminIds { get; set; }

}