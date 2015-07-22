using System.Collections;

namespace IoTSpike.IoTClient
{
    public interface IIoTClient
    {
        bool IsOpen { get; }

        string DeviceName { get; }

        string DeviceId { get; }

        void Open();

        void Close();

        void SendAsync(IDictionary bag);
    }
}
