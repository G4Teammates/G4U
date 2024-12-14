using Azure;
using OrderMicroservice.Models;
using OrderMicroservice.Models.Message;
using OrderMicroservice.Models.UserModel;
using OrderMicroservice.Repositories.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace OrderMicroservice.Repositories.Services
{
    public class Message : IMessage
    {
        public event Action<FindUsernameModel> OnFindUserModelResponseReceived;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly IModel _channel2;
        private readonly IModel _channel3;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer2ConnectionFactory server2Factory, RabbitMQServer3ConnectionFactory server3Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel2 = server2Factory.Factory.CreateConnection().CreateModel();
            _channel3 = server3Factory.Factory.CreateConnection().CreateModel();
        }
        //conn 3
        public void ReceiveMessageCheckPurchased()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Check_purchased_queue";

                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = _config["25"],
                    Password = _config["26"],
                    VirtualHost = _config["25"],
                    Port = 5672,
                    HostName = _config["27"]
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel3.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel3);

                consumer.Received += async (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);

                    CheckPurchaseReceive order = null;
                    try
                    {
                        order = JsonSerializer.Deserialize<CheckPurchaseReceive>(message);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Failed to deserialize message: " + ex.Message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("order received message: " + message); // Log raw message


                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoOrder = scope.ServiceProvider.GetRequiredService<IOrderService>();

                            // Check if cateID exists in products
                            bool isPur = await IsPurchaseAsync(repoOrder, order);

                            // Send a response after processing
                            var response = new CheckPurchaseSending
                            {
                                IsPurchased = isPur
                            };

                            SendingMessage3(response, "Check_purchased", "Check_purchased_confirm_queue", "Check_purchased_confirm_queue", ExchangeType.Direct, true, false, false, false);
                            var jsonString = JsonSerializer.Serialize(response);
                            Console.WriteLine("Category sending message: " + jsonString); // Log raw message

                        }
                    }
                };
                _channel3.BasicConsume(
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


        //conn 3
        public void ReceiveMessageFromUser()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "updateUserName_queue_od";
               /* var connectionFactory = new ConnectionFactory
                {
                    UserName = _config["31"],
                    Password = _config["32"],
                    VirtualHost = _config["31"],
                    Port = 5672,
                    HostName = _config["33"]
                };
                using var connection = connectionFactory.CreateConnection();
                using var channel = connection.CreateModel();*/

                var queue = _channel3.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel3);
                consumer.Received += async (model, EventArgs) =>
                {
                    var boby = EventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    // Giải quyết vấn đề chuỗi bị "escape"
                    if (message.StartsWith("\"") && message.EndsWith("\""))
                    {
                        // Nếu chuỗi bắt đầu và kết thúc bằng dấu ngoặc kép, hãy giải tuần tự hóa nó
                        message = JsonSerializer.Deserialize<string>(message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("order received message: " + message);

                        // Deserialize JSON message to object
                        var Response = JsonSerializer.Deserialize<UpdateUserNameModel>(message);

                        // Kiểm tra nếu deserialization thành công
                        if (Response != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var repo = scope.ServiceProvider.GetRequiredService<IOrderService>();

                                // Check if exists in products
                                var update = await UpdateUserName(repo, Response);

                            }
                            // Gán dữ liệu vào model
                            Console.WriteLine($"Old: {Response.OldUserName}, New: {Response.NewUserName}");
                        }

                        else
                        {
                            Console.WriteLine("Failed to deserialize message to Response.");
                        }
                    }
                };
                _channel3.BasicConsume(
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
        //conn 2
        public void ReceiveMessageStastisticalGroupByUserToOrder()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_order";

                /*var connectionFactory = new ConnectionFactory
                {
                    UserName = _config["25"],
                    Password = _config["26"],
                    VirtualHost = _config["25"],
                    Port = 5672,
                    HostName = _config["27"]
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
                    // Deserialize JSON message to CategoryDeleteResponse object

                    if (!string.IsNullOrEmpty(message))
                    {
                        /*message = JsonSerializer.Deserialize<string>(message);*/
                        Console.WriteLine("Order received message: " + message); // Log raw message

                        var StastisticalResponse = JsonSerializer.Deserialize<TotalGroupByUserResponse>(message);
                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoProducts = scope.ServiceProvider.GetRequiredService<IOrderService>();

                            // Check if cateID exists in products
                            OrderGroupByUserData responseDTO = await StastisticalGroupByUserToOrder(repoProducts, StastisticalResponse);

                            
                            SendingMessage2(responseDTO, "StastisticalGroupByUser", "Stastistical_groupby_user_order_data", "Stastistical_groupby_user_order_data", ExchangeType.Direct, true, false, false, false);
                            var jsonString = JsonSerializer.Serialize(responseDTO);
                            Console.WriteLine("Order sending message: " + jsonString); // Log raw message

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


        //sending message
        // conn 2
        public void SendingMessage2<T>(
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
        // conn 3
        public void SendingMessage3<T>(
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
                var channel = _channel3;

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

        //method
        private async Task<bool> IsPurchaseAsync(IOrderService repo, CheckPurchaseReceive order)
        {
            bool IsPurchased = await repo.CheckPurchaseAsync(order);

            return IsPurchased;
        }
        private async Task<OrderGroupByUserData> StastisticalGroupByUserToOrder(IOrderService repo, TotalGroupByUserResponse Response)
        {
            // Use the GetProductsByCategoryIdAsync method to get products associated with the category
            /*List<Products> products = await repo.GetProductsByCategoryNameAsync(categoryName);*/

            // Return false if there are products associated with the category
            /*return !products.Any();*/

            var response = await repo.Data(Response);
            if (response != null)
            {
                var data = new OrderGroupByUserData()
                {
                    Revenue = response.Revenue
                };
                return data;
            }
            else
            {
                return new OrderGroupByUserData() { };
            }
        }
        private async Task<ResponseModel> UpdateUserName(IOrderService repo, UpdateUserNameModel model)
        {
            ResponseModel response = new();
            try
            {
                var prods = await repo.UpdateUserName(model);
                if (prods != null)
                {
                    response.Result = prods.Result;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any order";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }




        #region Send and receive Message Export
        //receive data export from user conn 3
        public void ReceiveMessageExport()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "prepareData_for_export";

                var connectionFactory = new ConnectionFactory
                {
                    UserName = _config["31"],
                    Password = _config["32"],
                    VirtualHost = _config["31"],
                    Port = 5672,
                    HostName = _config["33"]
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

                consumer.Received += (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    if (!string.IsNullOrEmpty(message))
                    {
                        FindUsernameModel models = Newtonsoft.Json.JsonConvert.DeserializeObject<FindUsernameModel>(message);
                        Console.WriteLine($"Order service received message from User service." +
                            $"Have {models.UsersExport.Count} user(s) can export." +
                            $"Have {models.MissingUsers.Count} user(s) missing"); // Log raw message
                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            OnFindUserModelResponseReceived?.Invoke(models); // Gọi event để thông báo có phản hồi

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
        #endregion
    }
}
