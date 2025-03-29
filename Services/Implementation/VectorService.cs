using Common.Dto;
using Common.Helper.Interface;
using Common.Params;
using Repositories.HttpClients;
using Repositories.MyDbContext;
using Services.Interfaces;

namespace Services.Implementation;

public class VectorService : IVectorService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;
    private readonly IChatServiceApiClient _chatHttpClient;

    public VectorService(MyDbContext context, IUserHelper jwtHelper, IChatServiceApiClient chatHttpClient)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _chatHttpClient = chatHttpClient;
    }

    public async Task<ResultDTO> GetAllVectorCollections()
    {
        var response = await _chatHttpClient.GetAsync<ChatServiceHttpClientResultDto>("Vector/get_collections");
        return new ResultDTO()
        {
            IsSuccess = response.success,
            Data = response.data,
            Message = response.success ? response.message : null,
            ErrorMessage = !response.success ? response.message : null,
        };
    }


}
