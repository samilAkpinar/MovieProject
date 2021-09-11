﻿using Business.Abstract;
using Business.Models;
using Business.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MovieProject.Controllers
{
    [Authorize]
    [Route("api/v1/cast")]
    [ApiController]
    public class CastController : Controller
    {
        private readonly ICastService _castService;
        public CastController(ICastService castService)
        {
            _castService = castService;
        }

        /// <summary>
        /// Get populer cast
        /// </summary>
        /// <param name="movieId"></param>
        /// <returns>List Cast</returns>
        [HttpGet("get-populer-cast")]
        public async Task<BaseResponse<List<Cast>>> GetPopulerCast(int movieId)
        {
            
            BaseResponse<List<Cast>> response = new BaseResponse<List<Cast>>();

            var logger = NLog.LogManager.GetCurrentClassLogger();

            var castList = await _castService.GetPopulerCast(movieId);

            if(castList == null)
            {
                logger.Info("Cast list didn't send");
                response.Data = null;
                response.ErrorMessages = "Cast list didn't send";
                return response;
            }

            logger.Info("Cast list sent");

            response.Data = castList;
            response.ErrorMessages = null;
            return response;
        }


        /// <summary>
        /// Get cast by id
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="castId"></param>
        /// <returns>Cast</returns>
        [HttpGet("get-cast-by-id")]
        public async Task<BaseResponse<Cast>> GetCastById(int movieId, int castId)
        {
            BaseResponse<Cast> response = new BaseResponse<Cast>();

            var logger = NLog.LogManager.GetCurrentClassLogger();

            var castList = await _castService.GetPopulerCast(movieId);

            if(castList == null)
            {
                logger.Info("Cast didn't send");

                response.Data = null;
                response.ErrorMessages = "Cast didn't send";
                return response;
            }

            var cast = castList.FirstOrDefault<Cast>(c => c.castId == castId);

            logger.Info("Cast sent");

            response.Data = cast;
            response.ErrorMessages = null;
            return response;
        }

    }
}
