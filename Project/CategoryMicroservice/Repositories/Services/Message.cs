using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text;
using CategoryMicroservice.DBContexts.Entities;
using CategoryMicroservice.Models;

namespace CategoryMicroservice.Repositories.Services
{
    public class Message : IMessage
    {
        public event Action<CategoryDeleteResponse> OnCategoryResponseReceived;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly IModel _channel1;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer1ConnectionFactory server1Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel1 = server1Factory.Factory.CreateConnection().CreateModel();

        }
        //delete-cate conn1
        public void ReceiveMessage()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "delete_category_confirm_queue";
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

                    Console.WriteLine("Category received message: " + message);

                    // Deserialize JSON message to CategoryDeleteResponse object
                    var categoryResponse = JsonSerializer.Deserialize<CategoryDeleteResponse>(message);

                    // Kiểm tra nếu deserialization thành công
                    if (categoryResponse != null)
                    {
                        // Gán dữ liệu vào model
                        Console.WriteLine($"CateName: {categoryResponse.CateName}, CanDelete: {categoryResponse.CanDelete}");
                        OnCategoryResponseReceived?.Invoke(categoryResponse); // Gọi event để thông báo có phản hồi
                        // Thực hiện xử lý khác với categoryResponse nếu cần
                        // Ví dụ: gọi một dịch vụ khác để xử lý response
                    }
                    else
                    {
                        Console.WriteLine("Failed to deserialize message to CategoryDeleteResponse.");
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
        //check-exist-cate conn1
        public void ReceiveMessageCheckExist()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "CheckExistCategory_For_RreateProduct";

               /* var connectionFactory = new ConnectionFactory
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

                    ICollection<CategoryNameModel> categories = null;
                    try
                    {
                        categories = JsonSerializer.Deserialize<ICollection<CategoryNameModel>>(message);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Failed to deserialize message: " + ex.Message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Catgory received message: " + message); // Log raw message


                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoProducts = scope.ServiceProvider.GetRequiredService<ICategoryService>();

                            // Check if cateID exists in products
                            bool isExist = await IsExistCategoryAsync(repoProducts, categories);

                            // Send a response after processing
                            var response = new CategoryCheckExist
                            {
                                categories = categories,
                                IsExist = isExist
                            };

                            SendingMessage(response, "CheckExist", "CheckExistCategory_For_RreateProduct_Confirm", "CheckExistCategory_For_RreateProduct_Confirm", ExchangeType.Direct, true, false, false, false);
                            var jsonString = JsonSerializer.Serialize(response);
                            Console.WriteLine("Category sending message: " + jsonString); // Log raw message

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
        //sending message conn1
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


        //method
        private async Task<bool> IsExistCategoryAsync(ICategoryService repo, ICollection<CategoryNameModel> categories)
        {
            // Sử dụng phương thức CheckCategorysByCategoryNameAsync để kiểm tra các danh mục có tồn tại không
            bool allCategoriesExist = await repo.CheckCategorysByCategoryNameAsync(categories);

            // Trả về kết quả kiểm tra (true nếu tất cả danh mục tồn tại, false nếu có danh mục không tồn tại)
            return allCategoriesExist;
        }

    }
}
