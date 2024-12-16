using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text;
using ReportMicroservice.Models;

namespace ReportMicroservice.Repostories
{
    public class Message : IMessage
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly IModel _channel3;
        public Message(IServiceScopeFactory scopeFactory, IConfiguration config, RabbitMQServer3ConnectionFactory server3Factory)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _channel3 = server3Factory.Factory.CreateConnection().CreateModel();
        }
        // conn 3
        public void ReceiveMessageFromUser()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "updateUserName_queue_rp";
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
                        Console.WriteLine("report received message: " + message);

                        // Deserialize JSON message to object
                        var Response = JsonSerializer.Deserialize<UpdateUserNameModel>(message);

                        // Kiểm tra nếu deserialization thành công
                        if (Response != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var repo = scope.ServiceProvider.GetRequiredService<IRepo>();

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
        //method

        private async Task<ResponseDTO> UpdateUserName(IRepo repo, UpdateUserNameModel model)
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
                    response.Message = "Not found any Report";
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
