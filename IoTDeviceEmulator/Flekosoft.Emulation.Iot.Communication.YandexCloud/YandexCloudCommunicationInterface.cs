using System;
using System.Security.Cryptography.X509Certificates;
using Flekosoft.Common;
using Flekosoft.Common.Logging;
using Flekosoft.Emulation.Iot.Common;
using Flekosoft.Emulation.Iot.Common.Communication;

namespace Flekosoft.Emulation.Iot.Communication.YandexCloud
{
    public class YandexCloudCommunicationInterface : PropertyChangedErrorNotifyDisposableBase, ICommunicationInterface
    {
        private readonly MqttClient _mqttClient;
        private readonly string _deviceId;
        private readonly string _publishSuffix;

        public YandexCloudCommunicationInterface(string deviceId, string password, string publishSuffix, string subscribeSuffix, X509Certificate2 rootCa)
        {
            _deviceId = deviceId;
            _publishSuffix = publishSuffix;
            _mqttClient = new MqttClient();
            _mqttClient.MessageReceived += _mqttClient_MessageReceived;
            _mqttClient.Start("mqtt.cloud.yandex.net", 8883, Guid.NewGuid().ToString(), deviceId, password, rootCa, new string[] { $"$devices/{deviceId}/commands/{subscribeSuffix}#" });
        }

        private void _mqttClient_MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            NewDataFromServerEvent?.Invoke(this, new JsonStringEventArgs(payload));
        }

        public void SendJsonData(string jsonData)
        {
            if (_mqttClient.Publish($"$devices/{_deviceId}/events/{_publishSuffix}",
                System.Text.Encoding.UTF8.GetBytes(jsonData)))
            {
                AppendDebugLogMessage($"Publish one: \"{jsonData}\"");
            }
        }

        public event EventHandler<JsonStringEventArgs> NewDataFromServerEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mqttClient.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
