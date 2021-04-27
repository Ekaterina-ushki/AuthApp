using System.ComponentModel.DataAnnotations.Schema;
using AuthApp.Data.Infrastructure.Interfaces;
using AuthApp.Data.Models.Common;

namespace AuthApp.Data.Entities
{
    [Table("Tasks")]
    public class DashboardTask : IEntity
    {
        public int TaskId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskType TaskType { get; set; } = TaskType.Default;
        public TaskStatus TaskStatus { get; set; } = TaskStatus.ToDo;
    }
}