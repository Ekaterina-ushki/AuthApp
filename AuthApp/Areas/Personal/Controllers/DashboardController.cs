using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthApp.Application.Commands;
using AuthApp.Application.Models;
using AuthApp.Data;
using AuthApp.Data.Entities;
using AuthApp.Data.Models.Common;
using AuthApp.Data.Models.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskStatus = AuthApp.Data.Models.Common.TaskStatus;

namespace AuthApp.Areas.Personal.Controllers
{
    [Area("Personal")]
    [Authorize]
    [Route("/Personal/Account/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _dataContext;

        public DashboardController(ApplicationDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet, Route("")]
        public async Task<ActionResult> GetDashboard()
        {
            var identity = (ClaimsIdentity) User.Identity;
            var loginName = identity.Name.ToUpper();
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == loginName);
            var tasks = _dataContext.DashboardTasks.Where(x => x.OwnerId == user.Id).ToArray();
            var dashboard = new Dashboard
            {
                Tasks = tasks
            };

            return View("Dashboard", dashboard);
        }

        [HttpGet, Route("TaskSummary")]
        public async Task<ActionResult> TaskSummary(int? taskId = null)
        {
            var user = await GetUser();
            if (taskId == null)
            {
                var newTask = new AddOrUpdateTaskCommand()
                {
                    OwnerId = user.Id
                };
                FillSelected();
                return View("Task", newTask);
            }

            var task = await _dataContext.DashboardTasks.FirstOrDefaultAsync(x => x.TaskId == taskId);
            if (task.OwnerId != user.Id)
            {
                throw new HttpResponseException(StatusCodes.Status404NotFound);
            }

            var taskToEdit = new AddOrUpdateTaskCommand
            {
                OwnerId = task.OwnerId,
                Name = task.Name,
                Description = task.Description,
                TaskType = task.TaskType
            };

            FillSelected();
            return View("Task", taskToEdit);
        }

        [HttpPost, Route("UpdateTaskStatus")]
        public async Task<ActionResult> UpdateTaskStatus(int taskId, TaskStatus newStatus)
        {
            var user = await GetUser();
            var task = await _dataContext.DashboardTasks.FirstOrDefaultAsync(x => x.TaskId == taskId);
            if (task.OwnerId != user.Id)
            {
                throw new HttpResponseException(StatusCodes.Status404NotFound);
            }

            task.TaskStatus = newStatus;
            await _dataContext.SaveChangesAsync();

            return RedirectToAction("GetDashboard");
        }

        [HttpPost, Route("AddOrUpdateTask")]
        public async Task<ActionResult> AddOrUpdateTask(AddOrUpdateTaskCommand command)
        {
            if (!ModelState.IsValid)
            {
                FillSelected();
                return View("Task", command);
            }

            var user = await GetUser();
            var task = await _dataContext.DashboardTasks.FirstOrDefaultAsync(x => x.TaskId == command.TaskId);

            if (task == null)
            {
                var newTask = new DashboardTask
                {
                    OwnerId = user.Id,
                    Name = command.Name,
                    Description = command.Description,
                    TaskType = command.TaskType,
                    TaskStatus = TaskStatus.ToDo
                };

                await _dataContext.DashboardTasks.AddAsync(newTask);
                await _dataContext.SaveChangesAsync();
                return RedirectToAction("GetDashboard");
            }

            if (task.OwnerId != user.Id)
            {
                throw new HttpResponseException(StatusCodes.Status404NotFound);
            }

            task.OwnerId = user.Id;
            task.Name = command.Name;
            task.Description = command.Description;
            task.TaskType = command.TaskType;
            task.TaskStatus = TaskStatus.ToDo;

            await _dataContext.SaveChangesAsync();
            return RedirectToAction("GetDashboard");
        }

        private async Task<User> GetUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var loginName = identity.Name.ToUpper();
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == loginName);
            return user;
        }

        private void FillSelected()
        {
            var taskNames = Enum.GetValues(typeof(TaskType))
                .Cast<int>()
                .ToArray();

            var taskTypes = taskNames.Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = $"{(TaskType) x:F}"
            });

            ViewBag.TaskTypes = taskTypes;
        }
    }
}