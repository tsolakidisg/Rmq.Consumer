using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
            // Create a connection factory, using the default settings for local node
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "localhost",
                VirtualHost = "/",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();

            // Retrieve 1 message at a time from the Queue
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            // Informative console output -> comment out for debugging
            //int messageCount = Convert.ToInt16(channel.MessageCount(queueName));
            //Console.WriteLine("Listening to {0}. This channel has {1} messages on the queue.", queueName, messageCount);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, e) =>
            {
                string requestData = System.Text.Encoding.UTF8.GetString(e.Body.Span);
                RequestModel request = JsonConvert.DeserializeObject<RequestModel>(requestData);
                Console.WriteLine("Request received: " + request.ToString());

                WebRequest webRequest = WebRequest.Create($"https://localhost:44355/api/Orders/" + request.OrderID);
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                Stream datastream = webResponse.GetResponseStream();
                StreamReader reader = new StreamReader(datastream);
                string responseData = reader.ReadToEnd();
                FullModel responseFromServer = JsonConvert.DeserializeObject<FullModel>(responseData);

                ResponseModel response = new ResponseModel();

                if (responseFromServer.OrderStatus == "Submitted")
                {
                    response.Progress = "50%";
                } 
                else if(responseFromServer.OrderStatus == "Provision in progress")
                {
                    response.Progress = "75%";
                }
                else if (responseFromServer.OrderStatus == "Completed")
                {
                    response.Progress = "100%";
                }
                else
                {
                    response.Progress = "25%";
                }

                // Serialize the response as a JSON Object
                string responseMessage = JsonConvert.SerializeObject(response);

                // Create the Header field RequestId for the Response, based on the value retrieved from the Request
                var basicProperties = channel.CreateBasicProperties();
                basicProperties.Headers = new Dictionary<string, object>();
                basicProperties.Headers.Add("RequestId", e.BasicProperties.Headers["RequestId"]);

                // Publish the response message in the queue
                channel.BasicPublish(
                    string.Empty,
                    "consumerQueue",
                    basicProperties,
                    Encoding.UTF8.GetBytes(responseMessage));

                Console.WriteLine("Order received: " + request);
                // Ackowledge the message retrieval
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            };

            // Retrieve the Request message from the respective queue, without ackowledging it until it is processed
            channel.BasicConsume(queue: "publisherQueue", autoAck: false, consumer: consumer);
            Console.WriteLine("Connection closed, no more messages");
            Console.ReadLine();
        }
    }
}
