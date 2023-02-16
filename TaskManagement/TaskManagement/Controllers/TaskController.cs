using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IMapper _mapper;

        public TaskController(ITaskRepository taskRepository, IMapper mapper,ISectionRepository sectionRepository)
        {
            _taskRepository = taskRepository;
            _sectionRepository = sectionRepository;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(200, Type =typeof(IEnumerable<Models.Task>))]
        public IActionResult GetTasks()
        {
            try
            {
                var task = _mapper.Map<List<TaskDto>>(_taskRepository.GetTasks());
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(task);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        [HttpGet("{taskID}")]
        [ProducesResponseType(200, Type = typeof(Models.Task))]
        [ProducesResponseType(400)]
        public IActionResult GetTaskByID(int taskID)
        {
            try
            {
                if (!_taskRepository.TaskExists(taskID))
                {
                    return NotFound();
                }
                var task = _mapper.Map<TaskDto>(_taskRepository.GetTaskByID(taskID));
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                return Ok(task);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateTask([FromQuery] int userID, [FromQuery] int roleID,
            [FromQuery] int sectionID,[FromBody] TaskDto taskCreate) 
        {
           
                if (taskCreate == null)
                {
                    return BadRequest();
                }

                if (!_sectionRepository.SectionExists(sectionID))
                {
                    ModelState.AddModelError("", "The Section Not Already exists");
                    return NotFound();
                }
                if (_taskRepository.TaskExists(taskCreate.Id))
                {
                    ModelState.AddModelError("", "The Task  Already exists");
                    return StatusCode(402);
                }

                if (!ModelState.IsValid) return BadRequest(ModelState);
                var taskMap = _mapper.Map<Models.Task>(taskCreate);
                if (!_taskRepository.CreateTask(userID, roleID, taskMap))
                {
                    ModelState.AddModelError("", "Something wrong while created");
                    return StatusCode(500, ModelState);
                }
                return Ok("Created successfully");
            
            
            
        }

        [HttpPut("{taskID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(240)]
        [ProducesResponseType(404)]
        public IActionResult UpdateTask(int taskID,
            [FromForm] TaskDto taskUpdate)
        {
            try
            {
                if (!_taskRepository.TaskExists(taskID))
                    return NotFound();
                if (taskUpdate == null)
                    return BadRequest(ModelState);
                if (taskID != taskUpdate.Id)
                    return BadRequest(ModelState);

                var taskMap = _mapper.Map<Models.Task>(taskUpdate);
                if (!_taskRepository.UpdateTask(taskMap))
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

        [HttpDelete("{taskID}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(240)]
        public IActionResult DeleteTask( int taskID) 
        {
            try
            {
                if (!_taskRepository.TaskExists(taskID))
                    return NotFound();

                var task = _taskRepository.GetTaskByID(taskID);
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (!_taskRepository.DeleteTask(task))
                {
                    ModelState.AddModelError("", "Some thing wrong while deleted");
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
