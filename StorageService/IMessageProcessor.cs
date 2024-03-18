namespace StorageService
{
    public interface IMessageProcessor<T>
    {
        void Process(T message);
    }
}
