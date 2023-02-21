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
    public class WorkSpaceController : ControllerBase
    {
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public WorkSpaceController(IWorkSpaceRepository workSpaceRepository,
            IUserRepository userRepository , IMapper mapper)
        {
            _workSpaceRepository = workSpaceRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<WorkSpace>))]
        public IActionResult WorkSpaces() // lấy toàn bộ workspace 
        {
            try
            {
                var workSpaces = _mapper.Map<List<WorkSpaceDto>>
                (_workSpaceRepository.WorkSpaces());
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(workSpaces);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(WorkSpace))]
        [ProducesResponseType(400)]
        public IActionResult GetWorkSpaceByID(int id) // lấy workspace theo id
        {
            try
            {
                if (!_workSpaceRepository.WorkSpaceExists(id))
                    return NotFound();
                var workSpace = _mapper.Map<WorkSpaceDto>
                    (_workSpaceRepository.GetWorkSpaceByID(id));
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(workSpace);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        [HttpGet("{id}/sections")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Section>))]
        [ProducesResponseType(400)]
        public IActionResult GetSectionsByWorkSpace(int id)
        {
            try
            {
                if (!_workSpaceRepository.WorkSpaceExists(id))
                    return NotFound();
                var sections = _mapper.Map<List<SectionDto>>(_workSpaceRepository.GetSectionsByWorkSpace(id));
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(sections);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateWorkSpace([FromQuery] int userID, [FromQuery] int roleID,
            [FromBody] WorkSpaceDto workSpaceCreate) // tạo workspace
        {
            try
            {
                if (workSpaceCreate == null)
                    return BadRequest(ModelState);

                var workSpace = _workSpaceRepository.WorkSpaces()
                    .Where(w => w.Id == workSpaceCreate.Id).FirstOrDefault();
                if (workSpace != null)
                {
                    ModelState.AddModelError("", "The Workspace Already exists");
                    return StatusCode(402);
                }

                if (!ModelState.IsValid) return BadRequest(ModelState);

                var workspaceMap = _mapper.Map<WorkSpace>(workSpaceCreate);
                if (!_workSpaceRepository.CreateWorkSpace(userID, roleID, workspaceMap))
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

        [HttpPut("{workSpaceID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(240)]
        public IActionResult UpdateWorkSpace(int workSpaceID,
            [FromBody] WorkSpaceDto workSpaceUpdate)
        {
            try
            {
                if (!_workSpaceRepository.WorkSpaceExists(workSpaceID))
                    return NotFound();
                if (workSpaceUpdate == null)
                    return BadRequest(ModelState);
                if (workSpaceID != workSpaceUpdate.Id)
                    return BadRequest(ModelState);

                var workspaceMap = _mapper.Map<WorkSpace>(workSpaceUpdate);
                if (!_workSpaceRepository.UpdateWorkSpace(workspaceMap))
                {
                    ModelState.AddModelError("", "Something wrong while updated");
                    return StatusCode(500, ModelState);
                }
                return Ok("Updated successfully");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        [HttpDelete("{workSpaceID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteWorkSpace(int workSpaceID)
        {
            try
            {
                if (!_workSpaceRepository.WorkSpaceExists(workSpaceID))
                    return NotFound();
                var workspace = _workSpaceRepository.GetWorkSpaceByID(workSpaceID);

                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (!_workSpaceRepository.DeleteWorkSpace(workspace))
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

        [HttpGet("{workSpaceID}/members")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<User>))]
        public IActionResult GetUsersByWorkSpace(int workSpaceID)
        {
            if (!_workSpaceRepository.WorkSpaceExists(workSpaceID))
            {
                return NotFound();
            }

            var users = _mapper.Map<List<UserDto>>
                (_workSpaceRepository.GetUsersByWorkSpace(workSpaceID));

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(users);
        }


        [HttpPost("{workSpaceID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        public IActionResult AddMemberIntoWorkspace(int workSpaceID, string nameUser, int roleID)
        {
            if (!_workSpaceRepository.WorkSpaceExists(workSpaceID)
                || !_userRepository.UserNameExists(nameUser))
                return NotFound();

            if (!_workSpaceRepository.AddMemberIntoWorkspace(workSpaceID, nameUser, roleID))
            {
                ModelState.AddModelError("", "Something wrong while add member");
                return StatusCode(500, ModelState);
            }

            return Ok("Add member successfully");
        }

    }
}
