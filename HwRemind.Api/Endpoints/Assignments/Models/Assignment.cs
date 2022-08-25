using System.ComponentModel.DataAnnotations;

namespace HwRemind.Api.Endpoints.Assignments.Models
{
    public class Assignment : BaseAssignment
    {
        public int? userId { get; set; }
    }

    public class BaseAssignment
    {
        public int? id { get; set; }
        [Required]
        public string name { get; set; }
        public string description { get; set; }
        public DateTime dueAt { get; set; }
    }
}
