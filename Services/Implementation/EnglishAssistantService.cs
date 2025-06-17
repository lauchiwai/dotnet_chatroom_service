using Common.Params.EnglishAssistant;
using Repositories.HttpClients;
using Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Services.Implementation;

public class EnglishAssistantService : IEnglishAssistantService
{
    private readonly IStreamClient _streamClient;

    public EnglishAssistantService(
        IStreamClient streamClient)
    {
        _streamClient = streamClient;
    }

    public async Task WordAssistan(Stream outputStream, WordAssistanParams param, CancellationToken cancellationToken)
    {
        try
        {
            await _streamClient.PostStreamAsync(
                "/EnglishAssistant/stream_english_word_analysis",
                param,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    public async Task WordTranslate(Stream outputStream, WordAssistanParams param, CancellationToken cancellationToken)
    {
        try
        {
            await _streamClient.PostStreamAsync(
                "/EnglishAssistant/stream_english_word_translate",
                param,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }


    public async Task WordTips(Stream outputStream, WordAssistanParams param, CancellationToken cancellationToken)
    {
        try
        {
            await _streamClient.PostStreamAsync(
                "/EnglishAssistant/stream_english_word_tips",
                param,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    public async Task TextLinguisticAssistant(Stream outputStream, TextLinguisticAssistantParams param, CancellationToken cancellationToken)
    {
        try
        {
            await _streamClient.PostStreamAsync(
                "/EnglishAssistant/stream_english_text_linguistic_analysis",
                param,
                outputStream,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            await SendErrorEvent(outputStream, ex.Message);
        }
    }

    private async Task SendErrorEvent(Stream stream, string message)
    {
        var errorEvent = new
        {
            code = 500,
            message,
            eventType = "system_error"
        };

        var eventData = $"event: error\ndata: {JsonSerializer.Serialize(errorEvent)}\n\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(eventData));
        await stream.FlushAsync();
    }
}
