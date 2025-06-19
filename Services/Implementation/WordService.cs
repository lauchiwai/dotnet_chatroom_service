using Common.Dto;
using Common.Helper.Implementation;
using Common.Helper.Interface;
using Common.Models;
using Common.Params.Word;
using Common.ViewModels.Word;
using Microsoft.EntityFrameworkCore;
using Repositories.MyDbContext;
using Services.Interfaces;

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

    public async Task<ResultDTO> GetWordList()
    {
        var result = new ResultDTO { IsSuccess = true };
        try
        {
            var userInfo = _jwtHelper.ParseToken<JwtUserInfo>();
            var words = await _userWordsRepository.GetQueryable()
                .Include(uw => uw.Word)
                .Where(uw => uw.UserId == userInfo.UserId)
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

            result.Data = words;
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
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

}
