namespace Services.Schedule
{
    public class UpdateScheduleRq
    {
        public ScheduleDTO Schedule { get; set; }
        public int SelectedScheduleDetailId { get; set; }
    }
}
