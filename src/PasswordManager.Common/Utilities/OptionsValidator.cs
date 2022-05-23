using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace PasswordManager.Common.Utilities;


//todo move to library Junetic.AspNetCore.Common
public sealed class OptionsValidator<TOptions> : IValidateOptions<TOptions> where TOptions : class {

	public ValidateOptionsResult Validate(string name, TOptions options) {
		ValidationContext validationContext = new ValidationContext(options);
		List<ValidationResult> validationResults = new List<ValidationResult>();
		bool noValidationErrorsOccured = Validator.TryValidateObject(options, validationContext, validationResults, true);

		if(noValidationErrorsOccured) {
			return ValidateOptionsResult.Success;
		}

		IEnumerable<string> validationFailures = validationResults.Select(validationResult => validationResult.ErrorMessage);

		return ValidateOptionsResult.Fail(validationFailures);
	}

}