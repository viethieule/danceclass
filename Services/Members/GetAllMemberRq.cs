using System;

namespace Services.Members
{
    public class GetAllMemberRq
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int? DefaultPackageId { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
    }
}