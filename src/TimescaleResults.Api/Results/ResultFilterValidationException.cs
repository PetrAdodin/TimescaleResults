namespace TimescaleResults.Api.Results;

public sealed class ResultFilterValidationException(string message)
    : Exception(message);