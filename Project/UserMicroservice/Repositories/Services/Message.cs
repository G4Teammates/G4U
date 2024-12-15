using Azure;
using MongoDB.Bson;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Immutable;
using UserMicroservice.Models;
using UserMicroservice.Models.Message;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using Newtonsoft;
using System.Text;
namespace UserMicroservice.Repositories.Services
{
    public class Message : IMessage
    {
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


        //check-exist-user  conn 2
        public void ReceiveMessageCheckExist()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "CheckExistUserName_For_RreateProduct";

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
                        message = System.Text.Json.JsonSerializer.Deserialize<string>(message);
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("User received message: " + message); // Log raw message


                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoUser = scope.ServiceProvider.GetRequiredService<IUserService>();

                            // Check if cateID exists in products
                            bool isExist = await IsExistUserAsync(repoUser, message);

                            // Send a response after processing
                            var response = new UserCheckExist
                            {
                                IsExist = isExist
                            };

                            SendingMessage2(response, "CheckExist", "CheckExistUserName_For_RreateProduct_Confirm", "CheckExistUserName_For_RreateProduct_Confirm", ExchangeType.Direct, true, false, false, false);
                            var jsonString = System.Text.Json.JsonSerializer.Serialize(response);
                            Console.WriteLine("User sending message: " + jsonString); // Log raw message

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

                var jsonString = System.Text.Json.JsonSerializer.Serialize(message);
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

                var jsonString = System.Text.Json.JsonSerializer.Serialize(message);
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
        private async Task<bool> IsExistUserAsync(IUserService repo, string userName)
        {
            bool userExist = await repo.CheckUserByUserNameAsync(userName);
            return userExist;
        }




        // conn 3
        public void ReceiveMessageExport()
        {
            try
            {
                // tên cổng
                /*const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "findUser_for_export";

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


                    if (!string.IsNullOrEmpty(message))
                    {

                        // Use IServiceScopeFactory to create a scope for the scoped service IRepo_Products
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var repoUser = scope.ServiceProvider.GetRequiredService<IUserService>();

                            // Check if cateID exists in products
                            ExportResult result = JsonConvert.DeserializeObject<ExportResult>(message);
                            Console.WriteLine($"User service received request from Order service. Order service need {result.ExportProfits.Count} user(s) information");
                            ResponseModel response = await GetUserFromListUsername(repoUser, result);
                            if (!response.IsSuccess)
                            {
                                Console.WriteLine("Error: ", response.Message);
                            }
                            else
                            {
                                FindUsernameModel userDataExport = (FindUsernameModel)response.Result;
                                SendingMessage3<FindUsernameModel>(userDataExport, "Export", "prepareData_for_export", "prepareData_for_export", ExchangeType.Direct, true, false, false, false);
                                var jsonString = System.Text.Json.JsonSerializer.Serialize(response);
                                Console.WriteLine($"User service received message from Order service.Have {userDataExport.UsersExport.Count} user(s) can export.Have {userDataExport.MissingUsers.Count} user(s) missing"); // Log raw message
                            }
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

        private async Task<ResponseModel> GetUserFromListUsername(IUserService repo, ExportResult userName)
        {
            ResponseModel response = await repo.GetUserByListUsername(userName);
            return response;
        }

    }
}
