using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Repository;
using TaskManagement.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TaskManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IMapper _mapper;

        public TaskController(ITaskRepository taskRepository, IMapper mapper,ISectionRepository sectionRepository, IWorkSpaceRepository workSpaceRepository)
        {
            _taskRepository = taskRepository;
            _sectionRepository = sectionRepository;
            _mapper = mapper;
            _workSpaceRepository = workSpaceRepository;
        }

        [HttpGet]
        public IActionResult GetTasks()
        {
            var tasks = _taskRepository.GetTasks();
            return Ok(tasks);
        }


        [HttpGet("AssignedTasks/{userID}")]
        public IActionResult GetAssignedTasksForUser(int workspaceID,int userID)
        {
            var tasks = _taskRepository.GetAssignedTasksForUser(workspaceID,userID);
            return Ok(tasks);
        }

        [HttpGet("UnassignedTasks")]
        public IActionResult GetUnassignedTasks(int workspaceID)
        {
            var task = _taskRepository.GetUnassignedTasks(workspaceID);
            return Ok(task);
        }

        [HttpGet("NumberOfTask")]
        public IActionResult GetTaskCountOfUser(int workspaceID, int userId)
        {
            var count = _taskRepository.GetTaskCountOfUser(workspaceID, userId);
            return Ok(count);   
        }
        [HttpGet("NumberOfCompletedTask")]
        public IActionResult GetUserCompletedTaskCount(int workspaceID, int userId, bool status)
        {
            var task = _taskRepository.GetUserCompletedTaskCount(workspaceID, userId, status);
            return Ok(task);    
        }

        [HttpPost]
        public IActionResult CreateTask([FromQuery] int userID, [FromQuery] int roleID,
            [FromQuery]int? sectionID, [FromBody] TaskDto taskCreate)
        {
            
            var taskMap = _mapper.Map<Models.Task>(taskCreate);
            var created = _taskRepository.CreateTask(sectionID, userID, roleID, taskMap);
            return Ok(created);
        }

        [HttpPut("{taskID}")]
        public IActionResult UpdateTask(int taskID,[FromBody] TaskDto taskUpdate)
        {
            var taskMap = _mapper.Map<Models.Task>(taskUpdate);
            var update = _taskRepository.UpdateTask(taskMap);
            return Ok(update);
        }

        [HttpDelete("{taskID}")]
        public IActionResult DeleteTask(int taskID)
        {
            var task = _taskRepository.GetTaskByID(taskID);
            if (task == null) {
                return Ok(new ResponseObject
                {
                    Status = "404",
                    Message = "Not found",
                    Data = null
                });
            }
            return Ok(_taskRepository.DeleteTask(task));
        }

        

    }
}
