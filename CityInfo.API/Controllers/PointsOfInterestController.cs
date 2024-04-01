using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;


        public PointsOfInterestController(
            ILogger<PointsOfInterestController> logger,
            IMailService mailService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation(
                    $"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));
        }
        
        [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, 
            int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation(
                    $"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }
            // find point of interest
            var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId,pointOfInterestId);
            if (pointOfInterest == null)
                return NotFound();
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));

        }
        
        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
            int cityId,
            PointOfInterestForCreationDto pointOfInterest)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);

            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);
            
            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn);
        }
        
        // [HttpPut("{pointofinterestid}")]
        // public ActionResult UpdatePointOfInterest(
        //     int cityId,
        //     int pointofinterestid,
        //     PointOfInterestForUpdateDto pointOfInterest)
        // {
        //     var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //     if (city == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestid);
        //     if (pointOfInterestFromStore == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     pointOfInterestFromStore.Name = pointOfInterest.Name;
        //     pointOfInterestFromStore.Description = pointOfInterest.Description;
        //
        //     return NoContent();
        // }
        //
        // [HttpPatch("{pointofinterestid}")]
        // public ActionResult PartiallyUpdatePointOfInterest(
        //     int cityId,
        //     int pointofinterestid,
        //     JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        // {
        //     var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //     if (city == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointofinterestid);
        //     if (pointOfInterestFromStore == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
        //     {
        //         Name = pointOfInterestFromStore.Name,
        //         Description = pointOfInterestFromStore.Description
        //     };
        //     patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     if (!TryValidateModel(pointOfInterestToPatch))
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        //     pointOfInterestFromStore.Description = pointOfInterestFromStore.Description;
        //
        //     return NoContent();
        // }
        //
        // [HttpDelete("{pointOfInterestId}")]
        // public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        // {
        //     var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
        //     if (city == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointOfInterestId);
        //     if (pointOfInterestFromStore == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     city.PointsOfInterest.Remove(pointOfInterestFromStore);
        //     _mailService.Send("Point of interest deleted", 
        //         $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");
        //     return NoContent();
        // }
    }
}