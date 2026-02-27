using Academic.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(new { success = true });
        }

        return controller.StatusCode(MapStatusCode(result.Error?.Code), new
        {
            success = false,
            error = result.Error,
            validationErrors = result.ValidationErrors
        });
    }

    public static ActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(new { success = true, data = result.Value });
        }

        return controller.StatusCode(MapStatusCode(result.Error?.Code), new
        {
            success = false,
            error = result.Error,
            validationErrors = result.ValidationErrors
        });
    }

    private static int MapStatusCode(string? code)
        => code switch
        {
            "validation_error" => StatusCodes.Status400BadRequest,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "forbidden" => StatusCodes.Status403Forbidden,
            "not_found" => StatusCodes.Status404NotFound,
            "business_rule" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
}
