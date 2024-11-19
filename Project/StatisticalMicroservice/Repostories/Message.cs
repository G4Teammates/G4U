using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using StatisticalMicroservice.Models.DTO;
using StatisticalMicroservice.Model.Message;
using StatisticalMicroservice.DBContexts.Entities;

namespace StatisticalMicroservice.Repostories
{
    public class Message : IMessage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public event Action<OrderGroupByUserData> OnOrderResponseReceived;
        public event Action<ProductGroupByUserData> OnProductResponseReceived;
        public event Action<TotalGroupByUserResponse> OnFinalResponseReceived;
        public Message(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public void ReceiveMessageProduct()
        {
            try
            {
                Console.WriteLine("ReceiveMessageProduct is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalSold_totalProduct_for_stastistical";

                var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var queue = channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(channel);

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
                channel.BasicConsume(
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

        public void ReceiveMessageUser()
        {
            try
            {
                Console.WriteLine("ReceiveMessageUser is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalUser_for_stastistical";

                var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var queue = channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(channel);

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
                channel.BasicConsume(
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

        public void ReceiveMessageOrder()
        {
            try
            {
                Console.WriteLine("ReceiveMessageOrder is runing");
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "totalOrder_for_stastistical";

                var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var queue = channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(channel);

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
                channel.BasicConsume(
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
            SendingMessageStastisticalGroupByUserToProduct(message);
            SendingMessageStastisticalGroupByUserToOrder(message);
        }
        public void ReceiveMessageStastisticalGroupByUserToProduct()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_product_data";
                var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };

                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var queue = channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(channel);

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

                channel.BasicConsume(
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
        public void ReceiveMessageStastisticalGroupByUserToOrder()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_order_data";
                var connectionFactory = new ConnectionFactory
                {
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/",
                    Port = 5672,
                    HostName = "localhost"
                };

                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();

                var queue = channel.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(channel);

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

                channel.BasicConsume(
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
        private void SendingMessageStastisticalGroupByUserToProduct<T>(T message)
        {
            const string ExchangeName = "StastisticalGroupByUser";
            // tên queue
            const string QueueName = "Stastistical_groupby_user_product";

            ConnectionFactory factory = new()
            {
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                Port = 5672,
                HostName = "localhost"
            };
            using var conn = factory.CreateConnection();
            using (var channel = conn.CreateModel())
            {
                channel.ExchangeDeclare(
                                      exchange: ExchangeName,
                                      type: ExchangeType.Direct, // Lựa chọn loại cổng (ExchangeType)
                                      durable: true              // Khi khởi động lại có bị mất dữ liệu hay không( true là không ) 
                                    );

                // Khai báo hàng chờ
                var queue = channel.QueueDeclare(
                                        queue: QueueName, // tên hàng chờ
                                        durable: false, // khi khởi động lại có mất không
                                                        // hàng đợi của bạn sẽ trở thành riêng tư và chỉ ứng dụng của
                                                        // bạn mới có thể sử dụng. Điều này rất hữu ích khi bạn cần giới
                                                        // hạn hàng đợi chỉ cho một người tiêu dùng.
                                        exclusive: false,
                                        autoDelete: false, // có tự động xóa không
                                        arguments: ImmutableDictionary<string, object>.Empty);



                // Liên kết hàng đợi với tên cổng bằng rounting key
                channel.QueueBind(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: QueueName); // Routing Key phải khớp với tên hàng chờ

                var jsonString = JsonSerializer.Serialize(message);

                var Body = Encoding.UTF8.GetBytes(jsonString);

                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: QueueName,
                    mandatory: true,
                    basicProperties: null,
                    body: Body);
            };
        }
        private void SendingMessageStastisticalGroupByUserToOrder<T>(T message)
        {
            const string ExchangeName = "StastisticalGroupByUser";
            // tên queue
            const string QueueName = "Stastistical_groupby_user_order";

            ConnectionFactory factory = new()
            {
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                Port = 5672,
                HostName = "localhost"
            };
            using var conn = factory.CreateConnection();
            using (var channel = conn.CreateModel())
            {
                channel.ExchangeDeclare(
                                      exchange: ExchangeName,
                                      type: ExchangeType.Direct, // Lựa chọn loại cổng (ExchangeType)
                                      durable: true              // Khi khởi động lại có bị mất dữ liệu hay không( true là không ) 
                                    );

                // Khai báo hàng chờ
                var queue = channel.QueueDeclare(
                                        queue: QueueName, // tên hàng chờ
                                        durable: false, // khi khởi động lại có mất không
                                                        // hàng đợi của bạn sẽ trở thành riêng tư và chỉ ứng dụng của
                                                        // bạn mới có thể sử dụng. Điều này rất hữu ích khi bạn cần giới
                                                        // hạn hàng đợi chỉ cho một người tiêu dùng.
                                        exclusive: false,
                                        autoDelete: false, // có tự động xóa không
                                        arguments: ImmutableDictionary<string, object>.Empty);



                // Liên kết hàng đợi với tên cổng bằng rounting key
                channel.QueueBind(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: QueueName); // Routing Key phải khớp với tên hàng chờ

                var jsonString = JsonSerializer.Serialize(message);

                var Body = Encoding.UTF8.GetBytes(jsonString);

                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: QueueName,
                    mandatory: true,
                    basicProperties: null,
                    body: Body);
            };
        }

        #endregion
    }
}
