using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text;
using CommentMicroservice.Models.Message;
using RabbitMQ.Client.Events;
using CommentMicroservice.Models.DTO;

namespace CommentMicroservice.Repositories
{
    public class Message : IMessage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public event Action<CheckPurchasedResponse> OnResponseReceived;
        private readonly IConfiguration _config;
        private readonly IModel _channel3;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer3ConnectionFactory server3Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel3 = server3Factory.Factory.CreateConnection().CreateModel();
        }
        //conn 3
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
        //conn 3
        public void ReceiveMessageCheckPurchased()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Check_purchased_confirm_queue";

               /* var connectionFactory = new ConnectionFactory
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

                    CheckPurchasedResponse Commt = null;
                    try
                    {
                        Commt = JsonSerializer.Deserialize<CheckPurchasedResponse>(message);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Failed to deserialize message: " + ex.Message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Comment received message: " + message); // Log raw message


                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        // Gán dữ liệu vào model
                        Console.WriteLine($"IsPurchased: {Commt.IsPurchased}");
                        OnResponseReceived?.Invoke(Commt); // Gọi event để thông báo có phản hồi
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
                const string QueueName = "updateUserName_queue_cmt";
                /*var connectionFactory = new ConnectionFactory
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
                        Console.WriteLine("cmt received message: " + message);

                        // Deserialize JSON message to object
                        var Response = JsonSerializer.Deserialize<UpdateUserNameModel>(message);

                        // Kiểm tra nếu deserialization thành công
                        if (Response != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var repo = scope.ServiceProvider.GetRequiredService<IRepoComment>();

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
        private async Task<ResponseModel> UpdateUserName(IRepoComment repo, UpdateUserNameModel model)
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
                    response.Message = "Not found any cmt";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
    