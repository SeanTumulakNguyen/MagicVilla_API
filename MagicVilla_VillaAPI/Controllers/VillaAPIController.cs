using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
        {
            //_logger.Log("Getting all villas", "");
            IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
            return Ok(_mapper.Map<VillaDto>(villaList));
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
        public async Task<ActionResult> GetVilla(int id)
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
            return Ok(_mapper.Map<VillaDto>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaDto>> CreateVilla([FromBody] VillaCreateDto createDto)
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

            Villa model = _mapper.Map<Villa>(createDto);

            await _dbVilla.CreateAsync(model);
            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVilla(int id)
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
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVille(int id, [FromBody] VillaUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.Id)
            {
                return BadRequest();
            }

            Villa model = _mapper.Map<Villa>(updateDto);

            await _dbVilla.UpdateAsync(model);
            await _dbVilla.SaveAsync();

            return NoContent();
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
