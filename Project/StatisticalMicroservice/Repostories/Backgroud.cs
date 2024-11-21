namespace StatisticalMicroservice.Repostories
{
    public class Backgroud : BackgroundService
    {
        private readonly IMessage _messageComsumer;
        public Backgroud(IMessage messageComsumer)
        {
            _messageComsumer = messageComsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Chạy ReceiveMessage trong một tác vụ nền
            await Task.WhenAll(
                Task.Run(() => _messageComsumer.ReceiveMessageProduct(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageUser(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageOrder(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageStastisticalGroupByUserToOrder(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageStastisticalGroupByUserToProduct(), stoppingToken)
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
