namespace TT.Deliveries.Data.Models
{
    public class Order
    {
        public Order(string orderNumber, string sender)
        {
            this.OrderNumber = orderNumber;
            this.Sender = sender;
        }

        public string OrderNumber { get; set; }
        public string Sender { get; set; }
    }
}
