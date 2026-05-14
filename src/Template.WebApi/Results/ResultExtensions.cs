using Microsoft.AspNetCore.Mvc;
using Template.Application.Common;

namespace Template.WebApi.Http;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => ToValidationProblem(result.Error),
            ErrorType.NotFound => ToProblem(result.Error, StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => ToProblem(result.Error, StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Domain => ToProblem(result.Error, StatusCodes.Status400BadRequest, "Business rule violation"),
            _ => ToProblem(result.Error, StatusCodes.Status500InternalServerError, "Unexpected error")
        };
    }

    private static IResult ToValidationProblem(Error error)
    {
        var errors = (error.ValidationErrors ?? [])
            .GroupBy(validationError => validationError.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(validationError => validationError.Message).ToArray());

        return Microsoft.AspNetCore.Http.Results.ValidationProblem(
            errors,
            title: "Validation failed",
            detail: error.Message,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    private static IResult ToProblem(Error error, int statusCode, string title)
    {
        return Microsoft.AspNetCore.Http.Results.Problem(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message,
            Extensions = { ["code"] = error.Code }
        });
    }
}
