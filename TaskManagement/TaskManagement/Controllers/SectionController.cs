using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Repository;

namespace TaskManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IMapper _mapper;

        public SectionController(ISectionRepository sectionRepository, IMapper mapper
            , IWorkSpaceRepository workSpaceRepository)
        {
            _sectionRepository = sectionRepository;
            _workSpaceRepository = workSpaceRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type =typeof(IEnumerable<Section>))]
        public IActionResult GetSections()
        {
            try
            {
                var sections = _mapper.Map<List<SectionDto>>(_sectionRepository.GetSections());
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(sections);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type =typeof(Section))]
        [ProducesResponseType(400)]
        public IActionResult GetSectionById(int id)
        {
            try
            {
                if (!_sectionRepository.SectionExists(id))
                    return NotFound();

                var section = _mapper.Map<SectionDto>(_sectionRepository.GetSectionById(id));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(section);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Section))]
        [ProducesResponseType(400)]
        public IActionResult CreateSection([FromQuery] int workSpaceID, [FromQuery]int userID,
            [FromQuery]int roleID,[FromBody] SectionDto sectionCreate)
        {
            try
            {
                if (workSpaceID != sectionCreate.WorkSpaceId)
                    return BadRequest(ModelState);
                if (!_workSpaceRepository.WorkSpaceExists(workSpaceID))
                    return NotFound();
                if (sectionCreate == null)
                    return BadRequest(ModelState);

                var section = _sectionRepository.GetSections().Where(s => s.Id == sectionCreate.Id).FirstOrDefault();
                if (section != null)
                {
                    ModelState.AddModelError("", "The Section Already exists");
                    return StatusCode(402);
                }
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var sectionMap = _mapper.Map<Section>(sectionCreate);
                if (!_sectionRepository.CreateSection(userID, roleID, sectionMap))
                {
                    ModelState.AddModelError("", "Something wrong while created");
                    return StatusCode(500, ModelState);
                }
                return Ok("Created successfully");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
        }

        [HttpPut("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(240)]
        [ProducesResponseType(404)]
        public IActionResult UpdateSection(int id, [FromForm]SectionDto sectionUpdate)
        {
            try
            {
                if (sectionUpdate.Id != id || sectionUpdate == null)
                    return BadRequest(ModelState);
                if (!_sectionRepository.SectionExists(sectionUpdate.Id))
                    return NotFound();

                var sectionMap = _mapper.Map<Section>(sectionUpdate);
                if (!_sectionRepository.UpdateSection(sectionMap))
                {
                    ModelState.AddModelError("", "something wrong while updated");
                    return StatusCode(500);
                }

                return Ok("Update successfully");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        [HttpDelete("{sectionID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteWorkSpace(int sectionID)
        {
            try
            {
                if (!_sectionRepository.SectionExists(sectionID))
                    return NotFound();
                var section = _sectionRepository.GetSectionById(sectionID);

                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (!_sectionRepository.DeleteSection(section))
                {
                    ModelState.AddModelError("", "Something wrong while deleted");
                    return BadRequest(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
