using Common.Dto;
using Common.Params;

namespace Services.Interfaces;

public interface IVectorService
{

    /// <summary>
    /// 獲取所有向量集合
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetAllVectorCollections();
}
