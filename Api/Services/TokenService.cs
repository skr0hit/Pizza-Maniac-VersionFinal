﻿using Api.DataAccess.Data.Repository.IRepository;
using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.Api.Services
{
    public class TokenService : ITokenService
    {
        //Symmetric means we use same Key to both Enc and Decrypt
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        /// <summary>
        /// Used to create JWT Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string CreateToken(AppUser user)
        {
            //Identify what claims to put in the token
            var claims = new List<Claim>
            {
                //We will use the NameID to store the username
                new Claim(JwtRegisteredClaimNames.NameId, user.AppUserName)
            };

            //Sign our token
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

            //Now we need to describe our token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Here we specify what goes inside our token
                Subject = new ClaimsIdentity(claims),

                //Expires in 7 days
                Expires = DateTime.Now.AddDays(7),

                //Pass our signing credentials
                SigningCredentials = creds
            };

            //Create Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //Create the token by passing it the descriptor we created above.
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //Return token
            return tokenHandler.WriteToken(token);
        }
    }
}
