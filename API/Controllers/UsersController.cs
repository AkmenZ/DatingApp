using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IUserRepository _userReppository;
        public readonly IMapper _mapper;
        public UsersController(IUserRepository userReppository, IMapper mapper)
        {
            _mapper = mapper;
            _userReppository = userReppository;
        }

        [HttpGet]//Get all users asynchronously
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await _userReppository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]//Get single user by id asynchronously
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await _userReppository.GetMemberAsync(username);
        }
    }
}