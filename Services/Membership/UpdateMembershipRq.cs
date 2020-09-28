using System;

namespace Services.Membership
{
    public class UpdateMembershipRq
    {
        public int UserId { get; set; }
        public int? RemainingSessions { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}