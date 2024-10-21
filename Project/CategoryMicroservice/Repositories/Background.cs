﻿using CategoryMicroservice.Repositories.Interfaces;

namespace CategoryMicroservice.Repositories
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
            await Task.Run(() =>
            {
                _messageComsumer.ReceiveMessage();
            }, stoppingToken);

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