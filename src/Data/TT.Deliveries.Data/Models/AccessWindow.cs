namespace TT.Deliveries.Data.Models
{
    using System;

    public class AccessWindow
    {
        public AccessWindow(DateTime startTime, DateTime endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
