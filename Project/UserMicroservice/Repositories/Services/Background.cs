﻿using UserMicroservice.Repositories.Interfaces;

namespace UserMicroservice.Repositories.Services
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
                Task.Run(() => _messageComsumer.ReceiveMessageCheckExist(), stoppingToken),
                Task.Run(() => _messageComsumer.ReceiveMessageExport(), stoppingToken)
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
