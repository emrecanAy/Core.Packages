using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.Serilog;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using MediatR;

namespace Core.Application.Pipelines.Logging;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILoggableRequest
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerServiceBase _loggerServiceBase;

    public LoggingBehavior(IHttpContextAccessor httpContextAccessor, LoggerServiceBase loggerServiceBase)
    {
        _httpContextAccessor = httpContextAccessor;
        _loggerServiceBase = loggerServiceBase;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var logParameters = new List<LogParameter>()
        {
            new LogParameter{ Type = request.GetType().Name, Value = request},
        };

        var logDetail = new LogDetail()
        {
            Method = next.Method.Name,
            Parameters = logParameters,
            User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "?"
        };

        _loggerServiceBase.Info(JsonSerializer.Serialize(logDetail));
        return await next();
    }
}
