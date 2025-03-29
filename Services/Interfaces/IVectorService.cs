﻿using Common.Dto;
using Common.Params;

namespace Services.Interfaces;

public interface IVectorService
{
    /// <summary>
    /// 創建 向量集合
    /// </summary>
    /// <param name="generateCollectionParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateVectorCollection(GenerateCollectionParams generateCollectionParams);

    /// <summary>
    /// 新增文本到 向量集合 
    /// </summary>
    /// <param name="upsertVectorCollectionParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> UpsertVectorCollectionTexts(UpsertVectorCollectionParams upsertVectorCollectionParams);

    /// <summary>
    /// 從向量集合 搜尋
    /// </summary>
    /// <param name="vectorSearchParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> VectorSemanticSearch(VectorSearchParams vectorSearchParams);

    /// <summary>
    /// 獲取所有向量集合
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetAllVectorCollections();
}
