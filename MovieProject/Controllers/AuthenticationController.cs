﻿using Business.Abstract;
using Business.Concrete;
using Business.Models;
using Business.Responses;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MovieProject.Controllers
{
    [Authorize]
    [Route("api/v1/authentication")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IJwtAuthenticationService _jwtAuthenticationService;
        

        public AuthenticationController(IAuthenticationService authenticationService, IJwtAuthenticationService jwtAuthenticationService)
        {
            _authenticationService = authenticationService;
            _jwtAuthenticationService = jwtAuthenticationService;
            
        }

      
        //Reister aşamasında angularda faklı bir class daha üret
        /// <summary>
        /// Sign up 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public BaseResponse<string> Register([FromBody] Users user)
        {
            BaseResponse<string> response = new BaseResponse<string>();

            var data = _authenticationService.Register(user);

            if (data)
            {
                //true
                response.Data = "";
                response.ErrorMessages = null;
            }
            else
            {
                //false
                response.Data = null;
                response.ErrorMessages = "User didn't add";
            }

            return response;
             
        }

        
        /// <summary>
        /// login with email and password
        /// </summary>
        /// <param name="user"></param>
        /// <returns>BaseResponse</returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public BaseResponse<User> Authenticate([FromBody] User user)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            BaseResponse<User> response = new BaseResponse<User>();

            
            var findUser = _authenticationService.Authenticate(user);

            if (findUser == null)
            {
                response.Data = null;
                response.ErrorMessages = "User not Found";
                return response;
            }

            var token = _jwtAuthenticationService.Authenticate(findUser);
            if(token == null)
            {
                
                logger.Info("Access Denied, User info: "+findUser.Email);

                //response.Data = Unauthorized();
                response.Data = null;
                response.ErrorMessages = "Access Denied";
                return response;
            }

            logger.Info("User token added, User info: " + findUser.Email);

            findUser.Token = token;

            response.Data = findUser;
            response.ErrorMessages = null;
            return response; 

        }

        /// <summary>
        /// Create film token
        /// </summary>
        /// <returns>BaseResponse</returns>
        [AllowAnonymous]
        [HttpGet("create-token")]
        public async Task<string> CreateToken()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("User create movie-token");

            
            return await _authenticationService.CreateToken();
        }

        /// <summary>
        /// User create session after film token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>String session value</returns>
        [AllowAnonymous]
        [HttpPost("create-session")]
        public Task<string> CreateSession([FromBody] CreateSession token)
        {
            return _authenticationService.CreateSession(token);
        }

        /// <summary>
        /// User validation email for create session
        /// </summary>
        /// <param name="validationEmail"></param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPost("validation-email")]
        public BaseResponse<bool>  CheckEmail([FromBody] ValidationEmail validationEmail )
        {
            BaseResponse<bool> response = new BaseResponse<bool>();

            var logger = NLog.LogManager.GetCurrentClassLogger();

            var isSuccess = _authenticationService.ValidationEmail(validationEmail);

            if (!isSuccess)
            {
                logger.Info("Email didn't send to user");
            }

            logger.Info("Email sent to user");

            response.Data = isSuccess;
            response.ErrorMessages = null;

            return response;
        }

    }
}