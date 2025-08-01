namespace OnlineJobPortal.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int JobSeekerId { get; set; }  // who receives the notification
        public string Message { get; set; }
        public DateTime NotificationDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }

}
