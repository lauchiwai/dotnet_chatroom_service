using Microsoft.AspNetCore.Mvc;

namespace Common.Dto;

public class StreamedResult : IActionResult
{
    private readonly Func<Stream, CancellationToken, Task> _streamWriter;
    private readonly string _contentType;

    public StreamedResult(Func<Stream, CancellationToken, Task> streamWriter, string contentType)
    {
        _streamWriter = streamWriter;
        _contentType = contentType;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = _contentType;
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        response.Headers.XContentTypeOptions = "no";

        await _streamWriter(response.Body, context.HttpContext.RequestAborted);
    }
}
