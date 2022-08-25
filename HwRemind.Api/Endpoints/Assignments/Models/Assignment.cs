using System.ComponentModel.DataAnnotations;

namespace HwRemind.Api.Endpoints.Assignments.Models
{
    public class Assignment
    {
        public int? id { get; set; }
        public int? userId { get; set; }
        [Required]
        public string name { get; set; }
        public string description { get; set; }
        public DateTime dueAt { get; set; }
    }
}
