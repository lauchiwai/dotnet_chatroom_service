using Common.Dto;
using Common.Helper.Interface;
using Common.HttpClientResultDto;
using Common.Params.Vector;
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

    public async Task<ResultDTO> CheckVectorDataExist(CheckVectorDataExistParams checkVectorDataExistParams)
    {
        var (statusCode, response) = await _httpClient.PostWithStatusAsync<CheckVectorDataExistParams, ChatServiceHttpClientResultDto>(
            "Vector/check_vector_data_exist",
            checkVectorDataExistParams
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }

    public async Task<ResultDTO> GetAllVectorCollections()
    {
        var (statusCode, response) = await _httpClient.GetWithStatusAsync<ChatServiceHttpClientResultDto>(
            "Vector/get_collections"
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }

    public async Task<ResultDTO> VectorSemanticSearch(VectorSearchParams vectorSearchParams)
    {
        var (statusCode, response) = await _httpClient.PostWithStatusAsync<VectorSearchParams, ChatServiceHttpClientResultDto>(
            "Vector/collections/search",
            vectorSearchParams
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }

    public async Task<ResultDTO> UpsertVectorCollectionTexts(UpsertVectorCollectionParams upsertVectorCollectionParams)
    {
        var (statusCode, response) = await _httpClient.PostWithStatusAsync<UpsertVectorCollectionParams, ChatServiceHttpClientResultDto>(
            "Vector/collections/upsert",
            upsertVectorCollectionParams
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }

    public async Task<ResultDTO> GenerateVectorCollection(GenerateCollectionParams generateCollectionParams)
    {
        var (statusCode, response) = await _httpClient.PostWithStatusAsync<GenerateCollectionParams, ChatServiceHttpClientResultDto>(
            "Vector/generate_collections",
            generateCollectionParams
        );

        return new ResultDTO()
        {
            IsSuccess = response.success,
            Code = (int)statusCode,
            Data = response.data,
            Message = response.message ?? string.Empty
        };
    }
}
