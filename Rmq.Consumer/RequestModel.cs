namespace Rmq.Consumer
{
    public class RequestModel
    {
        public int OrderID { get; set; }
        public string OrderStatus { get; set; }

        public RequestModel()
        {

        }

        public RequestModel(int orderId, string orderStatus)
        {
            this.OrderID = orderId;
            this.OrderStatus = orderStatus;
        }
    }
}