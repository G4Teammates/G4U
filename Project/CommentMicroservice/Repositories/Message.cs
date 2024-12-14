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
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }
        public void SendingMessageCheckPurchased<T>(T message)
        {
            // tên cổng
            const string ExchangeName = "Check_purchased";
            // tên queue
            const string QueueName = "Check_purchased_queue";

            ConnectionFactory factory = new()
            {
                UserName = _config["25"],
                Password = _config["26"],
                VirtualHost = _config["25"],
                Port = 5672,
                HostName = _config["27"]
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
        public void ReceiveMessageCheckPurchased()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "Check_purchased_confirm_queue";

                var connectionFactory = new ConnectionFactory
                {
                    UserName = _config["25"],
                    Password = _config["26"],
                    VirtualHost = _config["25"],
                    Port = 5672,
                    HostName = _config["27"]
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
        public void ReceiveMessageFromUser()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "updateUserName_queue_cmt";
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
    