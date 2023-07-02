namespace TT.Deliveries.Data.Models
{
    using System;

    using TT.Deliveries.Data.Dto;

    public class Delivery
    {
        public Guid Id { get; set; }

        public DeliveryState State { get; set; }

        public AccessWindow AccessWindow { get; set; }

        public Recipient Recipient { get; set; }

        public Order Order { get; set; }
    }
}
