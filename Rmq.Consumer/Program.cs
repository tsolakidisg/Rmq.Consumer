using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rmq.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.HostName = "localhost";
            factory.VirtualHost = "/";
            factory.Port = 5672;
            factory.UserName = "guest";
            factory.Password = "guest";

            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            //int messageCount = Convert.ToInt16(channel.MessageCount(queueName));
            //Console.WriteLine("Listening to {0}. This channel has {1} messages on the queue.", queueName, messageCount);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, e) =>
            {
                string requestData = System.Text.Encoding.UTF8.GetString(e.Body.Span);
                OrderPublish request = JsonConvert.DeserializeObject<OrderPublish>(requestData);
                Console.WriteLine("Request received: " + request.ToString());

                OrderConsume response = new OrderConsume();

                if (request.OrderStatus == "Submitted")
                {
                    response.Progress = "50%";
                } 
                else if(request.OrderStatus == "Provision in progress")
                {
                    response.Progress = "75%";
                }
                else if (request.OrderStatus == "Completed")
                {
                    response.Progress = "100%";
                }
                else
                {
                    response.Progress = "25%";
                }

                string responseData = JsonConvert.SerializeObject(response);

                var basicProperties = channel.CreateBasicProperties();
                basicProperties.Headers = new Dictionary<string, object>();
                basicProperties.Headers.Add("RequestId", e.BasicProperties.Headers["RequestId"]);

                channel.BasicPublish(
                    "",
                    "consumerQueue",
                    basicProperties,
                    Encoding.UTF8.GetBytes(responseData));

                Console.WriteLine("Order received: " + request);
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: "publisherQueue", autoAck: false, consumer: consumer);
            Console.WriteLine("Connection closed, no more messages");
            Console.ReadLine();
        }
    }
}
