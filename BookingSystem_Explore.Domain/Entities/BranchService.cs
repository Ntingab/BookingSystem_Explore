namespace BookingSystem_Explore.Domain.Entities
{
    public class BranchService
    {
        public Guid BranchServiceId { get; set; }
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public Branch? Branch { get; set; }
        public Service? Service { get; set; }
        public ICollection<AppointmentSlot>? AppointmentSlots { get; set; }
    }
}
