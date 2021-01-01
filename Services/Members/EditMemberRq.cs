using System;
using System.ComponentModel.DataAnnotations;

namespace Services.Members
{
    public class EditMemberRq
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string UserName { get; set; }
        public DateTime? Birthdate { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}