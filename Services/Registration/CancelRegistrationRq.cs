namespace Services.Registration
{
    public class CancelRegistrationRq
    {
        public int RegistrationId { get; set; }
        public bool? IsDelete { get; set; }
    }
}