using System.Collections.Generic;

namespace Lombiq.EInvoiceValidator.Models;

public class SchematronValidationResult
{
    public IList<FailedAssert> WarningFailedAsserts { get; } = [];
    public IList<FailedAssert> ErrorFailedAsserts { get; } = [];
}
