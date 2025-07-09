using Common.Params.Search;
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
    public async Task<IActionResult> GetWordList([FromQuery] SearchParams? searchParams = null)
    {
        var result = await _wordService.GetWordList(searchParams);

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

    [HttpPatch("UpdateWordReviewStatus/{wordId}")]
    [Authorize]
    public async Task<IActionResult> UpdateWordReviewStatus(int wordId)
    {
        var result = await _wordService.UpdateWordReviewStatus(wordId);
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

    [HttpDelete("RemoveWordByText")]
    [Authorize]
    public async Task<IActionResult> RemoveWordByText([FromQuery] string word)
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

    [HttpGet("CheckUserWordExistsByText")]
    [Authorize]
    public async Task<IActionResult> CheckUserWordExistsByText([FromQuery] string word)
    {
        var result = await _wordService.CheckUserWordExistsByText(word);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetNextReviewWord/{wordId}")]
    [Authorize]
    public async Task<IActionResult> GetNextReviewWord(int wordId)
    {
        var result = await _wordService.GetNextReviewWord(wordId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetReviewWordCount")]
    [Authorize]
    public async Task<IActionResult> GetReviewWordCount()
    {
        var result = await _wordService.GetReviewWordCount();
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
