﻿using Business.Abstract;
using Business.Models;
using Business.Responses;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieProject.Result;
using System.Net;

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


        /// <summary>
        /// Sign up 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public ServiceResult<string> Register([FromBody] Users user)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var data = _authenticationService.Register(user);

            if (!data)
            {
                logger.Error("User didn't add");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "User didn't add");
            }

            logger.Info("User added");
            return ServiceResult<string>.CreateResult(data.ToString());
        }


        /// <summary>
        /// login with email and password
        /// </summary>
        /// <param name="user"></param>
        /// <returns>BaseResponse</returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ServiceResult<User> Authenticate([FromBody] User user)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var findUser = _authenticationService.Authenticate(user);

            if (findUser == null)
            {
                logger.Error("User didn't find ");
                return ServiceResult<User>.CreateError(HttpStatusCode.BadRequest, "User didn't find");
            }

            var token = _jwtAuthenticationService.Authenticate(findUser);

            if (token == null)
            {

                logger.Error("Access Denied, User info: " + findUser.Email);
                return ServiceResult<User>.CreateError(HttpStatusCode.BadRequest, "Access Denied");
            }

            logger.Info("User token added, User info: " + findUser.Email);

            findUser.Token = token;

            return ServiceResult<User>.CreateResult(findUser);

        }

        /// <summary>
        /// Create film token
        /// </summary>
        /// <returns>BaseResponse</returns>
        [AllowAnonymous]
        [HttpGet("create-token")]
        public ServiceResult<string> CreateToken()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var token = _authenticationService.CreateToken().Result;

            if (token == null)
            {
                logger.Error("Movie token didn't creat");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "Movie token didn't creat");
            }

            logger.Info("Movie token created");
            return ServiceResult<string>.CreateResult(token);
        }

        /// <summary>
        /// User create session after film token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>String session value</returns>
        [AllowAnonymous]
        [HttpPost("create-session")]
        public ServiceResult<string> CreateSession([FromBody] CreateSession token)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var data = _authenticationService.CreateSession(token).Result;

            if (data == null)
            {
                logger.Error("Session didn't creat");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "Session didn't creat");
            }

            logger.Info("Session created");
            return ServiceResult<string>.CreateResult(data);
        }

        /// <summary>
        /// Create session with login
        /// </summary>
        /// <param name="sessionWithLogin"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("create-session-with-login")]
        public ServiceResult<SessionWithLoginResponse> CreateSessionWithLogin([FromBody] SessionWithLogin sessionWithLogin)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var result = _authenticationService.CreateSessionWithLogin(sessionWithLogin).Result;

            if (result == null)
            {
                logger.Error("Invalid session value");
                return ServiceResult<SessionWithLoginResponse>.CreateError(HttpStatusCode.BadRequest, "Invalid session value");
            }

            logger.Info("Valid session value created");
            return ServiceResult<SessionWithLoginResponse>.CreateResult(result);
        }

        /// <summary>
        /// User validation email for create session
        /// </summary>
        /// <param name="validationEmail"></param>
        /// <returns>Boolean</returns>
        [AllowAnonymous]
        [HttpPost("validation-email")]
        public ServiceResult<string> CheckEmail([FromBody] ValidationEmail validationEmail)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var isSuccess = _authenticationService.ValidationEmail(validationEmail);

            if (!isSuccess)
            {
                logger.Error("Email didn't send to user");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "Email didn't send to user");
            }

            logger.Info("Email sent to user");
            return ServiceResult<string>.CreateResult(isSuccess.ToString());

        }


        /// <summary>
        /// reset password
        /// </summary>
        /// <param name="email"></param>
        /// <returns>boolean</returns>
        [AllowAnonymous]
        [HttpGet("reset-password")]
        public ServiceResult<string> ResetPassword(string email)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var isSuccess = _authenticationService.ResetPassword(email);

            if (!isSuccess)
            {
                logger.Error("Password didn't reset");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "Password didn't reset");
            }

            logger.Info("Password reset");
            return ServiceResult<string>.CreateResult(isSuccess.ToString());
        }

        /// <summary>
        /// update password
        /// </summary>
        /// <param name="reset"></param>
        /// <returns>boolean</returns>
        [AllowAnonymous]
        [HttpPost("update-password")]
        public ServiceResult<string> AddNewPassword(ResetPassword reset)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var isSuccess = _authenticationService.UpdatePassword(reset);

            if (!isSuccess)
            {
                logger.Error("Password didn't update");
                return ServiceResult<string>.CreateError(HttpStatusCode.BadRequest, "Password didn't update");
            }

            logger.Info("Pasword updated");
            return ServiceResult<string>.CreateResult(isSuccess.ToString());
        }
    }
}