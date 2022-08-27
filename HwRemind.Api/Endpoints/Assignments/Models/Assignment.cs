using System.ComponentModel.DataAnnotations;

namespace HwRemind.Api.Endpoints.Assignments.Models
{
    public class Assignment : BaseAssignment
    {
        public int? userId { get; set; }
    }

    public class BaseAssignment
    {
        public BaseAssignment() { }
        public BaseAssignment(Assignment assignment) 
        {
            id = assignment.id;
            name = assignment.name;
            description = assignment.description;
            dueAt = assignment.dueAt;
        }

        public int? id { get; set; }
        [Required]
        public string name { get; set; }
        public string description { get; set; }
        public DateTime dueAt { get; set; }
    }
}
