using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
    {
    // [Route("api/[controller]")
    // above takes the name of the controller be default,
    // could cause issues if api name were to change,
    // ultimate changing the endpoint name
    [Route("/api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
        {
        protected ApiResponse _response;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
            {
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._response = new();
            }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetVillas()
            {

            try
                {
                //_logger.Log("Getting all villas", "");
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _response.Result = _mapper.Map<VillaDto>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
                }
            catch (Exception ex)
                {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                }
            return _response;

            }

        // if you do not define HTTP Verb, it defaults to HTTPGET
        [HttpGet("{id:int}", Name = "GetVilla")]
        // Using Status Codes
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        // Manual Way
        // [ProducesResponseType(200, Type =typeof(VillaDto))]
        // [ProducesResponseType(404)]
        // [ProducesResponseType(400)]
        public async Task<ActionResult<ApiResponse>> GetVilla(int id)
            {
            try
                {
                if (id == 0)
                    {
                    return BadRequest();
                    }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                    {
                    return NotFound();
                    }
                _response.Result = _mapper.Map<VillaDto>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
                }
            catch (Exception ex)
                {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                }
            return _response;
            }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> CreateVilla([FromBody] VillaCreateDto createDto)
            {
            try
                {
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDto.Name.ToLower()) != null)
                    {
                    ModelState.AddModelError("", "Villa Already Exists!");
                    return BadRequest(ModelState);
                    }
                if (createDto == null)
                    {
                    return BadRequest(createDto);
                    }

                Villa villa = _mapper.Map<Villa>(createDto);

                await _dbVilla.CreateAsync(villa);

                _response.Result = _mapper.Map<VillaDto>(villa);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
                }
            catch (Exception ex)
                {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                }
            return _response;
            }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> DeleteVilla(int id)
            {
            try
                {
                if (id == 0)
                    {
                    return BadRequest();
                    }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                    {
                    return NotFound();
                    }
                await _dbVilla.RemoveAsync(villa);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
                }
            catch (Exception ex)
                {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                }
            return _response;
            }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> UpdateVille(int id, [FromBody] VillaUpdateDto updateDto)
            {
            try
                {
                if (updateDto == null || id != updateDto.Id)
                    {
                    return BadRequest();
                    }

                Villa model = _mapper.Map<Villa>(updateDto);

                await _dbVilla.UpdateAsync(model);
                await _dbVilla.SaveAsync();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
                }
            catch (Exception ex)
                {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                }
            return _response;
            }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
            {
            if (patchDto == null || id == 0)
                {
                return BadRequest();
                }
            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            if (villa == null)
                {
                return BadRequest();
                }
            patchDto.ApplyTo(villaDto, ModelState);

            Villa model = _mapper.Map<Villa>(villaDto);

            await _dbVilla.UpdateAsync(model);
            await _dbVilla.SaveAsync();

            if (!ModelState.IsValid)
                {
                return BadRequest(ModelState);
                }
            return NoContent();
            }
        }
    }
