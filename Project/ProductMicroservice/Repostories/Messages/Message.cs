using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models.Message;
using ProductMicroservice.Models.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ProductMicroservice.Models;
using System;
using Azure;

namespace ProductMicroservice.Repostories.Messages
{
    public class Message : IMessage
    {

        private readonly IServiceScopeFactory _scopeFactory;
        public event Action<CategoryCheckExistResponse> OnCategoryResponseReceived;
        public event Action<UserCheckExistResponse> OnUserResponseReceived;
        public event Action<OrderItemsResponse> OnOrderItemsResponseReceived;
        private readonly IConfiguration _config;
        private readonly IModel _channel1;
        private readonly IModel _channel2;
        private readonly IModel _channel3;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer1ConnectionFactory server1Factory,
            RabbitMQServer2ConnectionFactory server2Factory,
            RabbitMQServer3ConnectionFactory server3Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel1 = server1Factory.Factory.CreateConnection().CreateModel();
            _channel2 = server2Factory.Factory.CreateConnection().CreateModel();
            _channel3 = server3Factory.Factory.CreateConnection().CreateModel();
        }
        #region delete_category message conn 1
        public void ReceiveMessage()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "delete_category_queue";

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

                var queue = _channel1.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel1);

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
                        Console.WriteLine("Product received message: " + message); // Log raw message


                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoProducts = scope.ServiceProvider.GetRequiredService<IRepoProduct>();

                            // Check if cateID exists in products
                            bool canDelete = await CanDeleteCategoryAsync(repoProducts, message);

                            // Send a response after processing
                            var response = new CategoryDeleteResponse
                            {
                                CateName = message,
                                CanDelete = canDelete
                            };

                            SendingMessage1(response, "delete_category", "delete_category_confirm_queue", "delete_category_confirm_queue", ExchangeType.Direct, true, false, false, false);
                            var jsonString = JsonSerializer.Serialize(response);
                            Console.WriteLine("Product sending message: " + jsonString); // Log raw message

                        }
                    }
                };
                _channel1.BasicConsume(
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

        #region checkExistCategory Message conn 1
        public void ReceiveMessageCheckExistCategory()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "CheckExistCategory_For_RreateProduct_Confirm";

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

                var queue = _channel1.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: ImmutableDictionary<string, object>.Empty);

                var consumer = new EventingBasicConsumer(_channel1);

                consumer.Received += (model, EventArgs) =>
                {
                    var body = EventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Product received message: " + message);

                    // Deserialize JSON message to CategoryDeleteResponse object
                    var categoryResponse = JsonSerializer.Deserialize<CategoryCheckExistResponse>(message);

                    // Kiểm tra nếu deserialization thành công
                    if (categoryResponse != null)
                    {
                        // Gán dữ liệu vào model
                        Console.WriteLine($"CateName: {categoryResponse.CategoryName}, IsExist: {categoryResponse.IsExist}");
                        OnCategoryResponseReceived?.Invoke(categoryResponse); // Gọi event để thông báo có phản hồi
                        // Thực hiện xử lý khác với categoryResponse nếu cần
                        // Ví dụ: gọi một dịch vụ khác để xử lý response
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize message to CategoryCheckExistResponse.");
                    }
                };

                _channel1.BasicConsume(
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

        #region checkExistUserName Message conn 2
        public void ReceiveMessageCheckExistUserName()
        {
            try
            {
                // tên cổng
                const string ExchangeName = "delete_category";
                // tên queue
                const string QueueName = "CheckExistUserName_For_RreateProduct_Confirm";

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

                    Console.WriteLine("Product received message: " + message);

                    // Deserialize JSON message to CategoryDeleteResponse object
                    var Response = JsonSerializer.Deserialize<UserCheckExistResponse>(message);

                    // Kiểm tra nếu deserialization thành công
                    if (Response != null)
                    {
                        // Gán dữ liệu vào model
                        Console.WriteLine($"IsExist: {Response.IsExist}");
                        OnUserResponseReceived?.Invoke(Response); // Gọi event để thông báo có phản hồi
                        // Thực hiện xử lý khác với categoryResponse nếu cần
                        // Ví dụ: gọi một dịch vụ khác để xử lý response
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize message to OnUserResponseReceived.");
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
        #endregion

        #region StastisticalGroupByUserToProduct conn 2
        public void ReceiveMessageStastisticalGroupByUserToProduct()
        {
            try
            {
                // tên cổng
               /* const string ExchangeName = "StastisticalGroupByUser";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_product";
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

                    if (!string.IsNullOrEmpty(message))
                    {
                        /*message = JsonSerializer.Deserialize<string>(message);*/
                        Console.WriteLine("Product received message: " + message); // Log raw message

                        // Deserialize JSON message to CategoryDeleteResponse object
                        var StastisticalResponse = JsonSerializer.Deserialize<TotalGroupByUserResponse>(message);

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoProducts = scope.ServiceProvider.GetRequiredService<IRepoProduct>();

                            // Check if cateID exists in products
                            ProductGroupByUserData responseDTO = await StastisticalGroupByUserToProduct(repoProducts, StastisticalResponse);

                            SendingMessage2(responseDTO, "StastisticalGroupByUser", "Stastistical_groupby_user_product_data", "Stastistical_groupby_user_product_data", ExchangeType.Direct, true, false, false, false);
                            var jsonString = JsonSerializer.Serialize(responseDTO);
                            Console.WriteLine("Product sending message: " + jsonString); // Log raw message

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

        #endregion

        #region receive-order-to-soldproduct conn 2
        public void ReceiveMessageSoldProduct()
        {
            try
            {
                // tên cổng
                const string ExchangeName = "delete_category";
                // tên queue
                const string QueueName = "order_for_sold_product";
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
                        Console.WriteLine("Product received message: " + message);

                        // Deserialize JSON message to CategoryDeleteResponse object
                        var orderResponse = JsonSerializer.Deserialize<OrderItemsResponse>(message);

                        // Kiểm tra nếu deserialization thành công
                        if (orderResponse != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var repo = scope.ServiceProvider.GetRequiredService<IRepoProduct>();

                                // Check if cateID exists in products
                                var update = await UpdateStastisticalOrder(repo, orderResponse);

                            }
                            // Gán dữ liệu vào model
                            Console.WriteLine($"CateName: {orderResponse.ProductSoldModels}, IsExist: {orderResponse.IsExist}");
                            OnOrderItemsResponseReceived?.Invoke(orderResponse); // Gọi event để thông báo có phản hồi
                                                                                 // Thực hiện xử lý khác với categoryResponse nếu cần
                                                                                 // Ví dụ: gọi một dịch vụ khác để xử lý response
                        }

                        else
                        {
                            Console.WriteLine("Failed to deserialize message to OrderItemsResponse.");
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
        #endregion

        #region ReceiveMessageFromUser _channel3
        public void ReceiveMessageFromUser()
        {
            try
            {
                // tên cổng
                const string ExchangeName = "delete_category";
                // tên queue
                const string QueueName = "updateUserName_queue_pro";
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
                        Console.WriteLine("Product received message: " + message);

                        // Deserialize JSON message to object
                        var Response = JsonSerializer.Deserialize<UpdateUserNameModel>(message);

                        // Kiểm tra nếu deserialization thành công
                        if (Response != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var repo = scope.ServiceProvider.GetRequiredService<IRepoProduct>();

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
        #endregion

        #region sending message
        //sending message
        // conn 1
        public void SendingMessage1<T>(
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
                var channel = _channel1;

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
        #endregion


        #region method and private model
        private async Task<bool> CanDeleteCategoryAsync(IRepoProduct repo, string categoryName)
        {
            // Use the GetProductsByCategoryIdAsync method to get products associated with the category
            List<Products> products = await repo.GetProductsByCategoryNameAsync(categoryName);

            // Return false if there are products associated with the category
            return !products.Any();
        }

        private async Task<ResponseDTO> UpdateUserName(IRepoProduct repo, UpdateUserNameModel model)
        {
            ResponseDTO response = new();
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
                    response.Message = "Not found any product";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        private async Task<ProductGroupByUserData> StastisticalGroupByUserToProduct(IRepoProduct repo, TotalGroupByUserResponse Response)
        {
            // Use the GetProductsByCategoryIdAsync method to get products associated with the category
            /*List<Products> products = await repo.GetProductsByCategoryNameAsync(categoryName);*/

            // Return false if there are products associated with the category
            /*return !products.Any();*/
            var response = await repo.Data(Response);
            if(response != null) 
            {
                var data = new ProductGroupByUserData()
                {
                    Solds = response.Solds,
                    Products = response.Products,
                    Views = response.Views
                };
                return data;
            }
            else
            {
                return new ProductGroupByUserData() { };
            }
        }

        private async Task<ResponseDTO> UpdateStastisticalOrder(IRepoProduct repo, OrderItemsResponse message)
        {
            ResponseDTO response = new();
            try
            {
                var Stas = await repo.UpdateRangeSoldAsync(message);
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

        #endregion
    }
}
