using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GQLServer.Models
{
    public class Recepie
    {
        public Recepie()
        {
            Steps = new List<Step>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public List<Step> Steps { get; set; }
    }
    
}