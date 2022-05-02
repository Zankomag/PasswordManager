
using PasswordManager.Common.Abstractions;

namespace PasswordManager.Application.Settings; 

public class ApplicationSettings : ISettings {

	public string SectionName => "Application";
	
	public long[] AdminIds { get; set; }

}