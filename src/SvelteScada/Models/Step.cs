using System.ComponentModel.DataAnnotations;

namespace GQLServer.Models
{
    public class Step
    {
        [Key]
        public int Id { get; set; }

        public string AirboxMode { get; set; }
    }
    
}