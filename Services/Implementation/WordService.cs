using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.Params.Search;
using Common.Params.Word;
using Common.ViewModels.Search;
using Common.ViewModels.Word;
using Microsoft.EntityFrameworkCore;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.Linq.Expressions;

namespace Services.Implementation;

public class WordService : IWordService
{
    private readonly MyDbContext _context;
    private readonly IUserHelper _jwtHelper;
    private readonly IRepository<Words> _wordRepository;
    private readonly IRepository<UserWords> _userWordsRepository;

    public WordService(
        MyDbContext context,
        IUserHelper jwtHelper,
        IRepository<Words> wordRepository,
        IRepository<UserWords> userWordsRepository)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _wordRepository = wordRepository;
        _userWordsRepository = userWordsRepository;
    }

    public async Task<ResultDTO> GetWordList(SearchParams? searchParams = null)
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var safeParams = searchParams ?? new SearchParams();

            safeParams.PageNumber = safeParams.PageNumber < 1 ? 1 : safeParams.PageNumber;
            safeParams.PageSize = safeParams.PageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => safeParams.PageSize
            };

            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var baseQuery = _userWordsRepository.GetQueryable()
                .Include(uw => uw.Word)
                .Where(uw => uw.UserId == userInfo.UserId);

            var filteredQuery = ApplyFilters(baseQuery, safeParams);

            var orderedQuery = ApplySorting(filteredQuery, safeParams.SortBy, safeParams.SortDirection);

            var totalCount = await orderedQuery.CountAsync();

            var pagedQuery = orderedQuery
                .Skip((safeParams.PageNumber - 1) * safeParams.PageSize)
                .Take(safeParams.PageSize);

            var items = await pagedQuery
                .Select(uw => new WordViewModel
                {
                    UserWordId = uw.UserWordId,
                    WordId = uw.WordId,
                    Word = uw.Word.Word,
                    NextReviewDate = uw.NextReviewDate,
                    LastReviewed = uw.LastReviewed,
                    ReviewCount = uw.ReviewCount
                })
                .ToListAsync();

            result.Data = new PagedViewModel<WordViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = safeParams.PageNumber,
                PageSize = safeParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)safeParams.PageSize)
            };
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    private IQueryable<UserWords> ApplyFilters(IQueryable<UserWords> query, SearchParams searchParams)
    {
        if (!string.IsNullOrWhiteSpace(searchParams.Keyword))
        {
            query = query.Where(uw => uw.Word.Word.Contains(searchParams.Keyword));
        }

        if (searchParams.StartDate.HasValue)
        {
            query = query.Where(uw => uw.CreatedAt >= searchParams.StartDate.Value);
        }
        if (searchParams.EndDate.HasValue)
        {
            query = query.Where(uw => uw.CreatedAt <= searchParams.EndDate.Value);
        }

        if (searchParams.CustomFilters != null)
        {
            if (searchParams.CustomFilters.TryGetValue("ReviewStatus", out var status))
            {
                if (status.ToString() == "NotReviewed")
                    query = query.Where(uw => uw.LastReviewed == null);
                else if (status.ToString() == "Reviewed")
                    query = query.Where(uw => uw.LastReviewed != null);
            }
        }

        return query;
    }

    private IQueryable<UserWords> ApplySorting(IQueryable<UserWords> query, string? sortBy, string? sortDirection)
    {
        sortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";

        switch (sortBy?.ToLower())
        {
            case "word":
                return sortDirection == "asc"
                    ? query.OrderBy(uw => uw.Word.Word)
                    : query.OrderByDescending(uw => uw.Word.Word);

            case "reviewdate":
                return sortDirection == "asc"
                    ? query.OrderBy(uw => uw.NextReviewDate)
                    : query.OrderByDescending(uw => uw.NextReviewDate);

            case "added":
                return sortDirection == "asc"
                    ? query.OrderBy(uw => uw.CreatedAt)
                    : query.OrderByDescending(uw => uw.CreatedAt);

            default:
                return query
                    .OrderBy(uw => uw.LastReviewed.HasValue)
                    .ThenBy(uw => uw.NextReviewDate);
        }
    }

    public async Task<ResultDTO> GetWordById(int wordId)
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var userWord = await _userWordsRepository.GetQueryable()
                .Include(uw => uw.Word)
                .FirstOrDefaultAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordId);

            if (userWord == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Word not found";
                return result;
            }

            result.Data = new WordViewModel
            {
                UserWordId = userWord.UserWordId,
                WordId = userWord.WordId,
                Word = userWord.Word.Word,
                NextReviewDate = userWord.NextReviewDate,
                LastReviewed = userWord.LastReviewed,
                ReviewCount = userWord.ReviewCount,
            };
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> AddWord(AddWordParams addParams)
    {
        var result = new ResultDTO { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var word = await _wordRepository.GetQueryable()
                .FirstOrDefaultAsync(w => w.Word == addParams.WordText);

            if (word == null)
            {
                word = new Words
                {
                    Word = addParams.WordText,
                    AddedAt = DateTime.UtcNow
                };
                await _wordRepository.AddAsync(word);
                await _context.SaveChangesAsync();
            }

            var exists = await _userWordsRepository.GetQueryable()
                .AnyAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == word.WordId);

            if (exists)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.Message = "Word already exists for user";
                return result;
            }

            var userWord = new UserWords
            {
                UserId = userInfo.UserId,
                WordId = word.WordId,
                NextReviewDate = DateTime.UtcNow.AddDays(1),
                ReviewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _userWordsRepository.AddAsync(userWord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> RemoveWordById(int wordId)
    {
        var result = new ResultDTO { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var userWord = await _userWordsRepository.GetQueryable()
                .FirstOrDefaultAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordId);

            if (userWord == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Word association not found";
                return result;
            }

            var otherUsersExist = await _userWordsRepository.GetQueryable()
                .AnyAsync(uw =>
                    uw.WordId == wordId &&
                    uw.UserId != userInfo.UserId);

            _userWordsRepository.Delete(userWord);

            if (!otherUsersExist)
            {
                var wordToDelete = await _wordRepository.GetQueryable()
                    .FirstOrDefaultAsync(w => w.WordId == wordId);

                if (wordToDelete != null)
                {
                    _wordRepository.Delete(wordToDelete);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> RemoveWordByText(string word)
    {
        var result = new ResultDTO { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var wordEntity = await _wordRepository.GetQueryable()
                .FirstOrDefaultAsync(w => w.Word == word);

            if (wordEntity == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Word not found";
                return result;
            }

            var userWord = await _userWordsRepository.GetQueryable()
                .FirstOrDefaultAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordEntity.WordId);

            if (userWord == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Word association not found";
                return result;
            }

            var otherUsersExist = await _userWordsRepository.GetQueryable()
                .AnyAsync(uw =>
                    uw.WordId == wordEntity.WordId &&
                    uw.UserId != userInfo.UserId);

            _userWordsRepository.Delete(userWord);
            if (!otherUsersExist)
            {
                _wordRepository.Delete(wordEntity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> UpdateWordReviewStatus(int wordId)
    {
        var result = new ResultDTO { IsSuccess = true };
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var userWord = await _userWordsRepository.GetQueryable()
                .FirstOrDefaultAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordId);
            if (userWord == null)
            {
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Word not found for user";
                return result;
            }

            userWord.LastReviewed = DateTime.UtcNow;
            userWord.ReviewCount += 1;

            userWord.NextReviewDate = CalculateNextReviewDate(userWord.ReviewCount);

            _userWordsRepository.Update(userWord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    private DateTime CalculateNextReviewDate(int reviewCount)
    {
        // 艾宾浩斯遗忘曲线
        return reviewCount switch
        {
            0 => DateTime.UtcNow.AddDays(1),
            1 => DateTime.UtcNow.AddDays(3),
            2 => DateTime.UtcNow.AddDays(7),
            3 => DateTime.UtcNow.AddDays(14),
            4 => DateTime.UtcNow.AddDays(30),
            _ => DateTime.UtcNow.AddDays(60)
        };
    }

    public async Task<ResultDTO> CheckUserWordExistsById(int wordId)
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var exists = await _userWordsRepository.GetQueryable()
                .AnyAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordId);

            result.Data = exists;
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> CheckUserWordExistsByText(string word)
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var wordEntity = await _wordRepository.GetQueryable()
                .FirstOrDefaultAsync(w => w.Word == word);

            if (wordEntity == null)
            {
                result.Code = 404;
                result.Data = false;
                return result;
            }

            var exists = await _userWordsRepository.GetQueryable()
                .AnyAsync(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.WordId == wordEntity.WordId);

            result.Data = exists;
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    public async Task<ResultDTO> GetNextReviewWord(int wordId)
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();

            var currentWord = await _userWordsRepository.GetQueryable()
                .Where(uw => uw.UserId == userInfo.UserId && uw.WordId == wordId)
                .Select(uw => new WordViewModel
                {
                    UserWordId = uw.UserWordId,
                    WordId = uw.WordId,
                    Word = uw.Word != null ? uw.Word.Word : null,
                    NextReviewDate = uw.NextReviewDate,
                    LastReviewed = uw.LastReviewed,
                    ReviewCount = uw.ReviewCount
                })
                .FirstOrDefaultAsync();

            if (currentWord == null)
            {
                result.IsSuccess = false;
                result.Message = "Word not found";
                return result;
            }

            WordViewModel? nextWord = null;

            if (currentWord.LastReviewed == null)
            {
                var unreviewedList = await _userWordsRepository.GetQueryable()
                    .Where(uw => uw.UserId == userInfo.UserId &&
                                uw.Word != null &&
                                uw.LastReviewed == null)
                    .OrderBy(uw => uw.UserWordId)
                    .Select(uw => new WordViewModel
                    {
                        UserWordId = uw.UserWordId,
                        WordId = uw.WordId,
                        Word = uw.Word != null ? uw.Word.Word : null,
                        NextReviewDate = uw.NextReviewDate,
                        LastReviewed = uw.LastReviewed,
                        ReviewCount = uw.ReviewCount
                    })
                    .ToListAsync();

                if (unreviewedList.Count > 0)
                {
                    var currentIndex = unreviewedList.FindIndex(w => w.WordId == wordId);

                    if (currentIndex == unreviewedList.Count - 1)
                    {
                        nextWord = await _userWordsRepository.GetQueryable()
                            .Where(uw => uw.UserId == userInfo.UserId &&
                                        uw.Word != null &&
                                        uw.LastReviewed != null)
                            .OrderBy(uw => uw.NextReviewDate)
                            .Select(uw => new WordViewModel
                            {
                                UserWordId = uw.UserWordId,
                                WordId = uw.WordId,
                                Word = uw.Word != null ? uw.Word.Word : null,
                                NextReviewDate = uw.NextReviewDate,
                                LastReviewed = uw.LastReviewed,
                                ReviewCount = uw.ReviewCount
                            })
                            .FirstOrDefaultAsync();
                    }
                    else if (currentIndex >= 0)
                    {
                        nextWord = unreviewedList[currentIndex + 1];
                    }
                    else
                    {
                        nextWord = unreviewedList.FirstOrDefault();
                    }
                }
            }
            else
            {
                nextWord = await _userWordsRepository.GetQueryable()
                    .Where(uw => uw.UserId == userInfo.UserId &&
                                 uw.Word != null &&
                                 uw.WordId != wordId &&
                                 uw.NextReviewDate > currentWord.NextReviewDate)
                    .OrderBy(uw => uw.NextReviewDate)
                    .Select(uw => new WordViewModel
                    {
                        UserWordId = uw.UserWordId,
                        WordId = uw.WordId,
                        Word = uw.Word != null ? uw.Word.Word : null,
                        NextReviewDate = uw.NextReviewDate,
                        LastReviewed = uw.LastReviewed,
                        ReviewCount = uw.ReviewCount
                    })
                    .FirstOrDefaultAsync();

                if (nextWord == null)
                {
                    nextWord = await _userWordsRepository.GetQueryable()
                        .Where(uw => uw.UserId == userInfo.UserId &&
                                    uw.Word != null &&
                                    uw.WordId != wordId)
                        .OrderBy(uw => uw.NextReviewDate)
                        .Select(uw => new WordViewModel
                        {
                            UserWordId = uw.UserWordId,
                            WordId = uw.WordId,
                            Word = uw.Word != null ? uw.Word.Word : null,
                            NextReviewDate = uw.NextReviewDate,
                            LastReviewed = uw.LastReviewed,
                            ReviewCount = uw.ReviewCount
                        })
                        .FirstOrDefaultAsync();
                }
            }

            if (nextWord == null)
            {
                nextWord = await _userWordsRepository.GetQueryable()
                    .Where(uw => uw.UserId == userInfo.UserId &&
                                 uw.Word != null &&
                                 uw.WordId != wordId)
                    .OrderBy(uw => uw.UserWordId)
                    .Select(uw => new WordViewModel
                    {
                        UserWordId = uw.UserWordId,
                        WordId = uw.WordId,
                        Word = uw.Word != null ? uw.Word.Word : null,
                        NextReviewDate = uw.NextReviewDate,
                        LastReviewed = uw.LastReviewed,
                        ReviewCount = uw.ReviewCount
                    })
                    .FirstOrDefaultAsync();

                if (nextWord != null)
                {
                    result.Message = "Showing fallback word";
                }
                else
                {
                    result.Message = "No words available";
                }
            }

            result.Data = nextWord;
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = $"Database error: {ex.GetBaseException()?.Message ?? ex.Message}";
        }
        return result;
    }

    public async Task<ResultDTO> GetReviewWordCount()
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var currentTime = DateTime.UtcNow;

            var count = await _userWordsRepository.GetQueryable()
                .Where(uw =>
                    uw.UserId == userInfo.UserId &&
                    uw.NextReviewDate <= currentTime)
                .CountAsync();

            result.Data = count;
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }
}
