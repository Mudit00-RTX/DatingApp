using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using API.Controllers.DTOs;
using API.Extensions;
using API.Entities;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;


        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);

        }
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]

        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if(user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);

            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return NotFound();
            var resullt = await _photoService.AddPhotoAsync(file);
            if(resullt.Error!= null) return BadRequest(resullt.Error.Message);
            var photo = new Photo
            {
                Url = resullt.SecureUrl.AbsoluteUri,
                PublicId = resullt.PublicId
            };

            if(user.Photos.Count==0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await _userRepository.SaveAllAsync()) 
            {
                return CreatedAtAction(nameof(GetUser),
                new{username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        
        public async Task<ActionResult> SetMainPhoto(int photoId)  {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());   //to get the user

            if(user == null) return NotFound();  // checking user availability

            var  photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);  //to hold the photo of the user  (1)

            if(photo == null) return NotFound();  //as FirstOrDefault can return null so to check whether it is null or not

            if(photo.IsMain) return BadRequest("this is already your main photo");  //to check if photo in (1) is already user's main photo, if it is, then it is not allowed

            var currentMain = user.Photos.FirstOrDefault(x=> x.IsMain);   // to check current photo of user and this must be switch to not be main photo

            if(currentMain !=null) currentMain.IsMain = false;  // means that we already have a photo that is set to main
            photo.IsMain = true;  //to set new photo or photo we are updating is main equal to true

            if(await _userRepository.SaveAllAsync()) return NoContent();  // save progress that is being tracked by Entity Framework and signed here to our database -- NoContent because it is not creating a new resource
            return BadRequest("Problem setting the main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error!=null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete photo!!");
        }
    }
}