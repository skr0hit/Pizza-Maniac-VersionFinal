﻿using AutoMapper;
using Api.DataAccess.Data;
using Api.DataAccess.Data.Repository.IRepository;
using Api.Models;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Api.Controllers
{
    public class AccountController : BaseApiController
    {
        //Unit of work to access DB
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        //Injecting my dependancies into the DI Container using Dependancy Injection.
        public AccountController(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Used for User Authentication and is set to run Asynchronously using features from the Task class in C#.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            //Get the user from the DB. Don't use FindAsync method as we are not searching by PK.
            var user = await _unitOfWork.AppUser.GetUserByUsernameAsync(loginDto.UserName);

            //Check if user exists and return message
            if (user == null)
                return Unauthorized("Invalid username");

            //Hash the inputted password by passing the SALT we have in DB
            using var hmac = new HMACSHA512(user.PasswordSalt);

            //Generate computed hash from inputted password
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            //Check if inputted password hash = password in DB's hash
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    //Return invalid password
                    return Unauthorized("Invalid password");
                }
            }

            //Passwords match
            return new AppUserDto
            {
                Username = user.AppUserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        /// <summary>
        /// Used to Register new users on the system. Since it also hits the DB, it is set to run asynchronously.
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        //Values could come from body or query. Web API will figure it out. Has to be an object not strings.
        public async Task<ActionResult<AppUserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.AppUserName))
                //Error 400 HTTP status code
                return BadRequest("Username is taken");

            //Provides us with our Hashing algorithm. Using will dispose of resources correctly.
            using var hmac = new HMACSHA512();


            //Create new user
            var user = new AppUser
            {
                AppUserName = registerDto.AppUserName.ToLower(),
                //Generate password Hash
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),

                //Generate password salt
                PasswordSalt = hmac.Key
            };

            //Map the input Dto to our User class
            _mapper.Map(registerDto, user);

            //Adding this to EF
            _unitOfWork.AppUser.Add(user);

            //Persist changes to DB
            await _unitOfWork.AppUser.SaveAllAsync();

            return new AppUserDto
            {
                Username = user.AppUserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        /// <summary>
        /// Check if user already exists in DB
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<bool> UserExists(string username)
        {
            //Search for user in DB
            var result = await _unitOfWork.AppUser.GetUserByUsernameAsync(username);

            //Conditional Operator to perform check
            return result != null ? true : false;
        }

    }
}
