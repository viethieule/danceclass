using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class DefaultPackage : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ICollection<Package> Packages { get; set; }
    }
}
