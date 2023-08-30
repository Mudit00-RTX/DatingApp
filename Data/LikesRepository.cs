using API.Controllers.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext __context;
        public LikesRepository(DataContext _context)
        {
            __context = _context;
            
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await __context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)  //userid can be sourceUserId or targetUserId
        {
            var users = __context.Users.OrderBy(u => u.UserName).AsQueryable();  // we are just getting a list of our users in the database ordered by their username AND AsQueryable means it has NOT been executed yet
            var likes = __context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked") {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            }

            if(likesParams.Predicate == "likedBy") {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers =  users.Select(user => new LikeDto {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });  // remove ToListAsync() as we do not want to execute. At this point we want to return a page list from this

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await __context.Users 
                        .Include(x =>x.LikedUsers)
                        .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}