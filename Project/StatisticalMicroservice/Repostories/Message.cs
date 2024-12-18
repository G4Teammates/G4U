﻿using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using StatisticalMicroservice.Models.DTO;
using StatisticalMicroservice.Model.Message;
using StatisticalMicroservice.DBContexts.Entities;
using Azure;
using static Google.Apis.Requests.BatchRequest;

namespace StatisticalMicroservice.Repostories
{
    public class Message : IMessage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public event Action<OrderGroupByUserData> OnOrderResponseReceived;
        public event Action<ProductGroupByUserData> OnProductResponseReceived;
        public event Action<TotalGroupByUserResponse> OnFinalResponseReceived;
        private readonly IConfiguration _config;
        private readonly IModel _channel2;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer2ConnectionFactory server1Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel2 = server1Factory.Factory.CreateConnection().CreateModel();
        }
        // conn 2
        public void ReceiveMessageProduct()
        {
            try
            {
                Console.WriteLine("ReceiveMessageProduct is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalSold_totalProduct_for_stastistical";

                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel2.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel2);

                consumer.Received += async (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    // Giải quyết vấn đề chuỗi bị "escape"
                    if (message.StartsWith("\"") && message.EndsWith("\""))
                    {
                        // Nếu chuỗi bắt đầu và kết thúc bằng dấu ngoặc kép, hãy giải tuần tự hóa nó
                        message = JsonSerializer.Deserialize<string>(message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Stastistical received message: " + message); // Log raw message

                        // Deserialize JSON message to CategoryDeleteResponse object
                        var ProductResponse = JsonSerializer.Deserialize<ProductResponse>(message);

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repo = scope.ServiceProvider.GetRequiredService<IRepo>();

                            // Check if cateID exists in products
                            var update = await UpdateStastistical(repo, ProductResponse);

                        }
                    }
                };
                _channel2.BasicConsume(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: consumer);
                // Giữ cho phương thức không kết thúc (lắng nghe liên tục)
                while (true)
                {
                    Thread.Sleep(100); // Bạn có thể dùng cách khác thay cho Thread.Sleep để không chặn luồng
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        // conn 2
        public void ReceiveMessageUser()
        {
            try
            {
                Console.WriteLine("ReceiveMessageUser is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalUser_for_stastistical";

                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel2.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel2);

                consumer.Received += async (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    // Giải quyết vấn đề chuỗi bị "escape"
                    if (message.StartsWith("\"") && message.EndsWith("\""))
                    {
                        // Nếu chuỗi bắt đầu và kết thúc bằng dấu ngoặc kép, hãy giải tuần tự hóa nó
                        message = JsonSerializer.Deserialize<string>(message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Stastistical received message: " + message); // Log raw message

                        // Deserialize JSON message to CategoryDeleteResponse object
                        var UserResponse = JsonSerializer.Deserialize<UserResponse>(message);

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repo = scope.ServiceProvider.GetRequiredService<IRepo>();

                            // Check if cateID exists in products
                            var update = await UpdateStastisticalUser(repo, UserResponse);

                        }
                    }
                };
                _channel2.BasicConsume(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: consumer);
                // Giữ cho phương thức không kết thúc (lắng nghe liên tục)
                while (true)
                {
                    Thread.Sleep(100); // Bạn có thể dùng cách khác thay cho Thread.Sleep để không chặn luồng
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        // conn 2
        public void ReceiveMessageOrder()
        {
            try
            {
                Console.WriteLine("ReceiveMessageOrder is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalOrder_for_stastistical";

                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel2.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel2);

                consumer.Received += async (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    // Giải quyết vấn đề chuỗi bị "escape"
                    if (message.StartsWith("\"") && message.EndsWith("\""))
                    {
                        // Nếu chuỗi bắt đầu và kết thúc bằng dấu ngoặc kép, hãy giải tuần tự hóa nó
                        message = JsonSerializer.Deserialize<string>(message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Stastistical received message: " + message); // Log raw message

                        // Deserialize JSON message to CategoryDeleteResponse object
                        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(message);

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repo = scope.ServiceProvider.GetRequiredService<IRepo>();

                            // Check if cateID exists in products
                            var update = await UpdateStastisticalOrder(repo, orderResponse);

                        }
                    }
                };
                _channel2.BasicConsume(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: consumer);
                // Giữ cho phương thức không kết thúc (lắng nghe liên tục)
                while (true)
                {
                    Thread.Sleep(100); // Bạn có thể dùng cách khác thay cho Thread.Sleep để không chặn luồng
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        
        public void SendingMessageStastisticalGroupByUser<T>(T message)
        {
            // conn 2
            SendingMessage(message, "StastisticalGroupByUser", "Stastistical_groupby_user_product", "Stastistical_groupby_user_product", ExchangeType.Direct, true, false, false, false);
            // conn 2
            SendingMessage(message, "StastisticalGroupByUser", "Stastistical_groupby_user_order", "Stastistical_groupby_user_order", ExchangeType.Direct, true, false, false, false);
        }
        // conn 2
        public void ReceiveMessageStastisticalGroupByUserToProduct()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_product_data";
                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };

                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel2.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel2);

                consumer.Received += (model, EventArgs) =>
                {
                    var body = EventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Category received message: " + message);

                    // Deserialize JSON message to CategoryDeleteResponse object
                    var Response = JsonSerializer.Deserialize<ProductGroupByUserData>(message);

                    // Kiểm tra nếu deserialization thành công
                    if (Response != null)
                    {
                        // Gán dữ liệu vào model
                        Console.WriteLine($"Total Products: {Response.Products}, Total Views: {Response.Views}, Total Solds: {Response.Solds}");
                        OnProductResponseReceived?.Invoke(Response); // Gọi event để thông báo có phản hồi
                        // Thực hiện xử lý khác với categoryResponse nếu cần
                        // Ví dụ: gọi một dịch vụ khác để xử lý response
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize message to OnProductResponseReceived.");
                    }
                };

                _channel2.BasicConsume(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: consumer);
                // Giữ cho phương thức không kết thúc (lắng nghe liên tục)
                while (true)
                {
                    Thread.Sleep(100); // Bạn có thể dùng cách khác thay cho Thread.Sleep để không chặn luồng
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        // conn 2
        public void ReceiveMessageStastisticalGroupByUserToOrder()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_order_data";
                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };

                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel2.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel2);

                consumer.Received += (model, EventArgs) =>
                {
                    var body = EventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Order received message: " + message);

                    // Deserialize JSON message to CategoryDeleteResponse object
                    var Response = JsonSerializer.Deserialize<OrderGroupByUserData>(message);

                    // Kiểm tra nếu deserialization thành công
                    if (Response != null)
                    {
                        // Gán dữ liệu vào model
                        Console.WriteLine($"Total revenue: {Response.Revenue}");
                        OnOrderResponseReceived?.Invoke(Response); // Gọi event để thông báo có phản hồi
                        // Thực hiện xử lý khác với categoryResponse nếu cần
                        // Ví dụ: gọi một dịch vụ khác để xử lý response
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize message to OnOrderResponseReceived.");
                    }
                };

                _channel2.BasicConsume(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: consumer);
                // Giữ cho phương thức không kết thúc (lắng nghe liên tục)
                while (true)
                {
                    Thread.Sleep(100); // Bạn có thể dùng cách khác thay cho Thread.Sleep để không chặn luồng
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #region method
        private async Task<ResponseDTO> UpdateStastistical( IRepo repo, ProductResponse message)
        {
            ResponseDTO response = new();
            try
            {
                var Stas = await repo.UpdateStastisticalProduct(message);
                if (Stas != null)
                {
                    response.Result = Stas.Result;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Statisticals";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ResponseDTO> UpdateStastisticalUser(IRepo repo, UserResponse message)
        {
            ResponseDTO response = new();
            try
            {
                var Stas = await repo.UpdateStastisticalUser(message);
                if (Stas != null)
                {
                    response.Result = Stas.Result;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Statisticals";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<ResponseDTO> UpdateStastisticalOrder(IRepo repo, OrderResponse message)
        {
            ResponseDTO response = new();
            try
            {
                var Stas = await repo.UpdateStastisticalOder(message);
                if (Stas != null)
                {
                    response.Result = Stas.Result;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Statisticals";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        //sending message
        public void SendingMessage<T>(
                                       T message,
                                       string exchangeName,
                                       string queueName,
                                       string routingKey,
                                       string exchangeType,
                                       bool exchangeDurable,
                                       bool queueDurable,
                                       bool exclusive,
                                       bool autoDelete)
        {
            try

            {
                // Sử dụng _channel1 (hoặc _channel2 nếu cần gửi qua server khác)
                var channel = _channel2;

                // Khai báo cổng Exchange
                channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: exchangeType,
                    durable: exchangeDurable
                );

                // Khai báo hàng chờ
                channel.QueueDeclare(
                    queue: queueName,
                    durable: queueDurable,
                    exclusive: exclusive,
                    autoDelete: autoDelete,
                    arguments: ImmutableDictionary<string, object>.Empty
                );

                Console.WriteLine($"Sending message to queue '{queueName}' on exchange '{exchangeName}': {message}");

                // Liên kết hàng đợi với cổng bằng routing key
                channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey
                );

                var jsonString = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonString);

                // Gửi message
                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: null,
                    body: body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the message: {ex.Message}");
            }
        }
        #endregion
    }
}
