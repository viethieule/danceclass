namespace Services.Package
{
    public class CreatePackageRq
    {
        public int UserId { get; set; }
        public int? DefaultPackageId { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
        public int RegisteredBranchId { get; set; }
    }
}