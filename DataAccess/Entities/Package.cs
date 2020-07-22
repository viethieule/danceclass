using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Package
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<ApplicationUser> MemberPackages { get; set; }
    }
}
