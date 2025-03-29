using Common.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VectorController : ControllerBase
{
    private readonly IVectorService _vectorService;

    public VectorController(IVectorService vectorService)
    {
        _vectorService = vectorService;
    }

    [HttpGet("GetAllVectorCollections")]
    [Authorize]
    public async Task<IActionResult> GetAllVectorCollections()
    {
        var result = await _vectorService.GetAllVectorCollections();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("VectorSemanticSearch")]
    [Authorize]
    public async Task<IActionResult> VectorSemanticSearch(VectorSearchParams vectorSearchParams)
    {
        var result = await _vectorService.VectorSemanticSearch(vectorSearchParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("UpsertVectorCollectionTexts")]
    [Authorize]
    public async Task<IActionResult> UpsertVectorCollectionTexts(UpsertVectorCollectionParams upsertVectorCollectionParams)
    {
        var result = await _vectorService.UpsertVectorCollectionTexts(upsertVectorCollectionParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("GenerateVectorCollection")]
    [Authorize]
    public async Task<IActionResult> GenerateVectorCollection(GenerateCollectionParams generateCollectionParams)
    {
        var result = await _vectorService.GenerateVectorCollection(generateCollectionParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
