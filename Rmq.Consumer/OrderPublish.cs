namespace Rmq.Consumer
{
    public class OrderPublish
    {
        public int OrderID { get; set; }
        public string OrderStatus { get; set; }

        public OrderPublish()
        {

        }

        public OrderPublish(int orderId, string orderStatus)
        {
            this.OrderID = orderId;
            this.OrderStatus = orderStatus;
        }
    }
}