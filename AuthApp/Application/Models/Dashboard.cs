using System.Collections.Generic;
using AuthApp.Data.Entities;

namespace AuthApp.Application.Models
{
    public class Dashboard
    {
        public ICollection<DashboardTask> Tasks { get; set; }
    }
}