using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using System.Security.Claims;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IUserRepository _userReppository;
        public readonly IMapper _mapper;
        public IPhotoService _photoService { get; }
        public UsersController(IUserRepository userReppository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userReppository = userReppository;
        }

        [HttpGet]//Get all users asynchronously
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userReppository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            var users = await _userReppository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]//Get single user by id asynchronously
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await _userReppository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDto)
        {
            var user = await _userReppository.GetUserByUsernameAsync(User.GetUsername());
            _mapper.Map(memberUpdateDto, user);
            _userReppository.Update(user);

            if(await _userReppository.SaveAllSync()) return NoContent();

            return BadRequest("Failed To Update User!");  
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userReppository.GetUserByUsernameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if(await _userReppository.SaveAllSync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName},_mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem Adding Photo!");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userReppository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This already is your main photo!");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain !=  null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await _userReppository.SaveAllSync()) return NoContent();

            return BadRequest("Failed To Set Main Photo!");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userReppository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("You Cannot Delete Main Photo!");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userReppository.SaveAllSync()) return Ok();

            return BadRequest("Failed To Delete Photo!");
        }
    }
}