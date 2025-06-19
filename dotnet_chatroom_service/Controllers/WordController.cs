using Common.Params.Word;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WordController : ControllerBase
{
    private readonly IWordService _wordService;

    public WordController(IWordService wordService)
    {
        _wordService = wordService;
    }

    [HttpGet("GetWordList")]
    [Authorize]
    public async Task<IActionResult> GetWordList()
    {
        var result = await _wordService.GetWordList();
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetWordById/{wordId}")]
    [Authorize]
    public async Task<IActionResult> GetWordById(int wordId)
    {
        var result = await _wordService.GetWordById(wordId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("AddWord")]
    [Authorize]
    public async Task<IActionResult> AddWord([FromBody] AddWordParams addParams)
    {
        var result = await _wordService.AddWord(addParams);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("RemoveWordByID/{wordId}")]
    [Authorize]
    public async Task<IActionResult> RemoveWordByID(int wordId)
    {
        var result = await _wordService.RemoveWordById(wordId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("RemoveWordByText/{word}")]
    [Authorize]
    public async Task<IActionResult> RemoveWordByText(string word)
    {
        var result = await _wordService.RemoveWordByText(word);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("CheckUserWordExistsById/{wordId}")]
    [Authorize]
    public async Task<IActionResult> CheckUserWordExistsById(int wordId)
    {
        var result = await _wordService.CheckUserWordExistsById(wordId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("CheckUserWordExistsByText/{word}")]
    [Authorize]
    public async Task<IActionResult> CheckUserWordExistsByText(string word)
    {
        var result = await _wordService.CheckUserWordExistsByText(word);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
