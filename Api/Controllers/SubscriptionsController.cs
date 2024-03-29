﻿using AutoMapper;
using Dapper;
using Api.DataAccess.Data.Repository.IRepository;
using Api.Models.Dtos;
using Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Api.Controllers
{
    public class SubscriptionsController : BaseApiController
    {
        //Unit of work to access DB
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        //Injecting my dependancies into the DI Container using Dependancy Injection.
        public SubscriptionsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        /// <summary>
        /// Get AppUser subscriptions
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubscriptionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] //Bad Request
        [ProducesResponseType(StatusCodes.Status404NotFound)] //Not Found
        [ProducesDefaultResponseType] //Any error that doesn't fall above
        [Authorize]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetAppUserSubscriptions(string username)
        {
            //Invalid username
            if (string.IsNullOrWhiteSpace(username))
            {
                //BadRequest 400
                return BadRequest("Invalid username.");
            }

            //Initialize dynamic parameters class located in Dapper namespace
            var parameters = new DynamicParameters();

            //Add input parameter
            parameters.Add("@AppUserName", username, DbType.String, ParameterDirection.Input);

            //Get User Subscriptions by calling stored proc asynchronously
            var subscriptions = await _unitOfWork.SP_Call.ReturnList<SubscriptionDto>(SD.sp_GetAppUserSubscriptions, parameters);


            //Wrap result in an OK response (Return HTTP reponse of 200 )
            return Ok(subscriptions);
        }
    }
}
