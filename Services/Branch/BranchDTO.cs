using System;
using System.Collections.Generic;
using DataAccess.Interfaces;
using Services.Members;

namespace Services.Branch
{
    public class BranchDTO : IAuditable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Address { get; set; }
        public List<MemberDTO> RegisteredMembers { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}