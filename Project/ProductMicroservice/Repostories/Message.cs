using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using ProductMicroservice.DBContexts.Entities;

namespace ProductMicroservice.Repostories
{
    public class Message : IMessage
    {

        private readonly IServiceScopeFactory _scopeFactory;

        public Message(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public void ReceiveMessage()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "delete_category_queue";

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
                    if (message != null)
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
                            
                            SendingMessage(response);
                            var jsonString = JsonSerializer.Serialize(response);
                            Console.WriteLine("Product sending message: " + jsonString); // Log raw message

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
        public void SendingMessage<T>(T message)
        {

            // tên cổng
            const string ExchangeName = "delete_category";
            // tên queue
            const string QueueName = "delete_category_confirm_queue";

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


        #region method and private model
        private async Task<bool> CanDeleteCategoryAsync(IRepoProduct repo, string categoryName)
        {
            // Use the GetProductsByCategoryIdAsync method to get products associated with the category
            List<Products> products = await repo.GetProductsByCategoryIdAsync(categoryName);

            // Return false if there are products associated with the category
            return !products.Any();
        }

        public class CategoryDeleteResponse
        {
            public string CateName { get; set; }
            public bool CanDelete { get; set; }
        }
        #endregion
    }
}
