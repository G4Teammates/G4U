﻿using OrderMicroservice.Models;
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
        public Message(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public void ReceiveMessageCheckPurchased()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Check_purchased_queue";

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

                            SendingMessageCheckPurchase(response);
                            var jsonString = JsonSerializer.Serialize(response);
                            Console.WriteLine("Category sending message: " + jsonString); // Log raw message

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

        public void SendingMessageCheckPurchase<T>(T message)
        {
            // tên cổng
            const string ExchangeName = "Check_purchased";
            // tên queue
            const string QueueName = "Check_purchased_confirm_queue";
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

        public void SendingMessageProduct<T>(T message)
        {
            // tên cổng
            const string ExchangeName = "Product";
            // tên queue
            const string QueueName = "order_for_sold_product";

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

        public void SendingMessageStatistiscal<T>(T message)
        {
            // tên cổng
            const string ExchangeName = "Stastistical";
            // tên queue
            const string QueueName = "totalOrder_for_stastistical";

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

        #region StastisticalGroupByUserToOrder
        public void ReceiveMessageStastisticalGroupByUserToOrder()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Stastistical_groupby_user_order";

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

                            SendingMessageStastisticalGroupByUserToOrder(responseDTO);
                            var jsonString = JsonSerializer.Serialize(responseDTO);
                            Console.WriteLine("Order sending message: " + jsonString); // Log raw message

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

        public void SendingMessageStastisticalGroupByUserToOrder<T>(T message)
        {

            // tên cổng
            const string ExchangeName = "StastisticalGroupByUser";
            // tên queue
            const string QueueName = "Stastistical_groupby_user_order_data";

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
                /*channel.ExchangeDeclare(
                                      exchange: ExchangeName,
                                      type: ExchangeType.Direct, // Lựa chọn loại cổng (ExchangeType)
                                      durable: true              // Khi khởi động lại có bị mất dữ liệu hay không( true là không ) 
                                    );*/

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




        #region Send and receive Message Export





        //send group by order message to user
        public void SendingMessageExport<T>(T message)
        {
            // tên cổng
            const string ExchangeName = "Export";
            // tên queue
            const string QueueName = "findUser_for_export";

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


        //receive data export from user
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

                consumer.Received += (sender, eventArgs) =>
                {
                    var boby = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(boby);
                    //Giải quyết vấn đề chuỗi bị "escape"
                    //if (message.StartsWith("\"") && message.EndsWith("\""))
                    //{
                    //    // Nếu chuỗi bắt đầu và kết thúc bằng dấu ngoặc kép, hãy giải tuần tự hóa nó
                    //    message = System.Text.Json.JsonSerializer.Deserialize<string>(message);
                    //}
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("User received message: " + message); // Log raw message
                        FindUsernameModel models = Newtonsoft.Json.JsonConvert.DeserializeObject<FindUsernameModel>(message);

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            //var repoUser = scope.ServiceProvider.GetRequiredService<IExportService>();
                            OnFindUserModelResponseReceived?.Invoke(models); // Gọi event để thông báo có phản hồi
                            // Check if cateID exists in products
                            //PrepareData(repoUser, models);
                            //var mes = Newtonsoft.Json.JsonConvert.SerializeObject(response.Message);
                            //Console.WriteLine("User received message finish. Message: " + mes); // Log raw message

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


        //private void PrepareData(IExportService repo, FindUsernameModel models)
        //{
        //     repo.PrepareData(models);
        //}
        #endregion
    }
}
