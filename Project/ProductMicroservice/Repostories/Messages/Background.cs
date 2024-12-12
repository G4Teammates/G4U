namespace ProductMicroservice.Repostories.Messages
{
    public class Background : BackgroundService
    {
        private readonly IMessage _messageComsumer;
        public Background(IMessage messageComsumer)
        {
            _messageComsumer = messageComsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Chạy ReceiveMessage trong một tác vụ nền
            await Task.WhenAll(
                Task.Run(() => _messageComsumer.ReceiveMessage(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageCheckExistCategory(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageCheckExistUserName(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageStastisticalGroupByUserToProduct(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageSoldProduct(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageFromUser(), stoppingToken)
            );


            // Giữ cho dịch vụ chạy liên tục
            while (!stoppingToken.IsCancellationRequested)
            {
                /*_messageComsumer.ReceiveMessage();*/
                // Delay để tránh vòng lặp liên tục
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
