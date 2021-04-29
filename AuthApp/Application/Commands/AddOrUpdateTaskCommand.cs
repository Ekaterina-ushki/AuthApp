using System.ComponentModel.DataAnnotations;
using AuthApp.Data.Models.Common;

namespace AuthApp.Application.Commands
{
    public class AddOrUpdateTaskCommand
    {
        public int? TaskId {get; set;}
        public int OwnerId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Name {get; set;}

        public string Description {get; set;}

        [Required]
        public TaskType TaskType {get; set;}
    }
}