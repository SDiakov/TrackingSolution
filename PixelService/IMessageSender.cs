using Common;

namespace PixelService
{
    public interface IMessageSender<T>
    {
        void SendMessage(T visit);
    }
}
