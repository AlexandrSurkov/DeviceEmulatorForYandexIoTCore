using System;

namespace Flekosoft.Emulation.Iot.Common
{
    public interface ICommunicationInterface : IDisposable
    {
        void SendJsonData(string jsonData);
        event EventHandler<JsonStringEventArgs> NewDataFromServerEvent;
    }
}
