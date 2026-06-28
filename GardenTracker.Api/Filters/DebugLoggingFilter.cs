using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GardenTracker.Api.Filters;

public class DebugLoggingFilter(ILogger<DebugLoggingFilter> logger) : IActionFilter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var args = JsonSerializer.Serialize(context.ActionArguments, JsonOptions);
            logger.LogDebug("=> Executing {ActionName} | Args: {Args}", actionName, args);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var actionName = context.ActionDescriptor.DisplayName;
            if (context.Exception != null)
            {
                logger.LogDebug(context.Exception, "<= Exception in {ActionName}", actionName);
            }
            else
            {
                var resultStr = context.Result is ObjectResult objResult 
                    ? JsonSerializer.Serialize(objResult.Value, JsonOptions) 
                    : context.Result?.GetType().Name;
                logger.LogDebug("<= Executed {ActionName} | Result: {Result}", actionName, resultStr);
            }
        }
    }
}
