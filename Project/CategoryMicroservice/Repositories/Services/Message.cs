using CategoryMicroservice.Models.DTO;
using CategoryMicroservice.Repositories.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text;

namespace CategoryMicroservice.Repositories.Services
{
    public class Message : IMessage
    {
        public event Action<CategoryDeleteResponse> OnCategoryResponseReceived;

        public void ReceiveMessage()
        {
            try
            {

                // tên cổng
                /* const string ExchangeName = "delete_category";*/
                // tên queue
                const string QueueName = "delete_category_confirm_queue";
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
            const string QueueName = "delete_category_queue";

            ConnectionFactory factory = new()
            {
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                Port = 5672,
                HostName = "localhost"
            };
            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                // khai bao cong Exchange
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

                Console.WriteLine("Category sending message: " + message); // Log raw message

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
    }
}
