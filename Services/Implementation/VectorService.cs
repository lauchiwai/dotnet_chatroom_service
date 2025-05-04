using Common.Dto;
using Common.Helper.Interface;
using Common.Params;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Services.Interfaces;

namespace Services.Implementation;

public class VectorService : IVectorService
{
    private readonly IApiClient _httpClient;

    public VectorService(MyDbContext context, IUserHelper jwtHelper, IApiClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultDTO> GetAllVectorCollections()
    {
        var response = await _httpClient.GetAsync<ChatServiceHttpClientResultDto>("Vector/get_collections");
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> VectorSemanticSearch(VectorSearchParams vectorSearchParams)
    {
        var response = await _httpClient.PostAsync<VectorSearchParams, ChatServiceHttpClientResultDto>("Vector/collections/search", vectorSearchParams);
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> UpsertVectorCollectionTexts(UpsertVectorCollectionParams upsertVectorCollectionParams)
    {
        var response = await _httpClient.PostAsync<UpsertVectorCollectionParams, ChatServiceHttpClientResultDto>("Vector/collections/upsert", upsertVectorCollectionParams);
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }

    public async Task<ResultDTO> GenerateVectorCollection(GenerateCollectionParams generateCollectionParams)
    {
        var response = await _httpClient.PostAsync<GenerateCollectionParams, ChatServiceHttpClientResultDto>("Vector/generate_collections", generateCollectionParams);
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.message
        };
    }
}
