namespace Template.Application.Common;

public sealed record ValidationError(string PropertyName, string Message);
