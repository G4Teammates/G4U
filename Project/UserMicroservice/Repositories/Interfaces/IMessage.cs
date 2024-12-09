namespace UserMicroservice.Repositories.Interfaces
{
    public interface IMessage
    {

        public void SendingMessageStatistiscal<T>(T message);

        public void SendingMessagePrepareDataExcel<T>(T message);
        public void ReceiveMessageExport();

        //checl-exist-user
        public void ReceiveMessageCheckExist();
        public void SendingMessageCheckExist<T>(T message);
        //updateusername
        public void SendingMessageUpdateUserNameCMT<T>(T message);
        public void SendingMessageUpdateUserNameRP<T>(T message);
        public void SendingMessageUpdateUserNameOD<T>(T message);
        public void SendingMessageUpdateUserNamePRO<T>(T message);
    }
}
