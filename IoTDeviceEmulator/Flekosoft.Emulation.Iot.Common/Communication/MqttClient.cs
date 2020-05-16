using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Flekosoft.Common;
using Flekosoft.Common.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;

namespace Flekosoft.Emulation.Iot.Common.Communication
{
    public class MqttClient : PropertyChangedErrorNotifyDisposableBase
    {
        private static int KeepAliveIntervalSec = 60;
        private static IMqttClient _mqttClient;
        private IMqttClientOptions _options;
        private string _brokerUrl;
        private int _port;
        private X509Certificate2 _serverRootCa;

        private readonly object _lockObject = new object();

        public MqttClient() : base("Mqtt client")
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttClient.UseDisconnectedHandler(MqttClient_Disconnected);
            _mqttClient.UseConnectedHandler(MqttClient_Connected);
            _mqttClient.UseApplicationMessageReceivedHandler(MqttClient_ApplicationMessageReceived);
        }

        public void Start(string brokerUrl, int port, string clientId, string username, string password, X509Certificate2 serverRootCA, string[] topics)
        {
            Start(brokerUrl, port, clientId, username, password, serverRootCA, null, topics);
        }

        public void Start(string brokerUrl, int port, string clientId, string username, string password, string[] topics)
        {
            Start(brokerUrl, port, clientId, username, password, null, null, topics);
        }

        public void Start(string brokerUrl, int port, string clientId, X509Certificate clientCertificate, X509Certificate2 serverRootCa, string[] topics)
        {
            Start(brokerUrl, port, clientId, string.Empty, string.Empty, serverRootCa, clientCertificate, topics);
        }

        public void Start(string brokerUrl, int port, string clientId, X509Certificate serverRootCa, string[] topics)
        {
            Start(brokerUrl, port, clientId, string.Empty, string.Empty, null, serverRootCa, topics);
        }

        private void Start(string brokerUrl, int port, string clientId, string username, string password, X509Certificate2 serverRootCa, X509Certificate clientCertificate, string[] topics)
        {
            _brokerUrl = brokerUrl;
            _port = port;
            Topics = topics;
            _serverRootCa = serverRootCa;

            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Start MQTT Client..." }, LogRecordLevel.Info));

            try
            {
                lock (_lockObject)
                {
                    var optionsBuilder = new MqttClientOptionsBuilder()
                        .WithClientId(clientId)
                        .WithTcpServer(brokerUrl, port)
                        .WithCleanSession()
                        //.WithKeepAliveSendInterval(TimeSpan.FromSeconds(KeepAliveIntervalSec)) // Keep alive send interval
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(KeepAliveIntervalSec * 1.5));      // Time after last keep alive when IoT Core will disconnect the client

                    if (clientCertificate == null)
                    {
                        //auth by username and password
                        optionsBuilder.WithCredentials(username, password);

                        if (serverRootCa != null)
                        {
                            MqttClientOptionsBuilderTlsParameters tlsOptions = new MqttClientOptionsBuilderTlsParameters();
                            tlsOptions.UseTls = true;
                            tlsOptions.AllowUntrustedCertificates = true;
                            //tlsOptions.IgnoreCertificateChainErrors = true;
                            //tlsOptions.IgnoreCertificateRevocationErrors = true;
                            tlsOptions.CertificateValidationCallback += MqttClient_CertificateValidationCallback;

                            optionsBuilder.WithTls(tlsOptions);
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Setup connection with username and password over TLS" }, LogRecordLevel.Info));
                        }
                        else Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Setup connection with username and password" }, LogRecordLevel.Info));

                    }
                    else
                    {
                        // Auth by client certificate
                        List<X509Certificate> certificates = new List<X509Certificate>();
                        certificates.Add(clientCertificate);

                        MqttClientOptionsBuilderTlsParameters tlsOptions = new MqttClientOptionsBuilderTlsParameters();
                        tlsOptions.Certificates = certificates;
                        tlsOptions.UseTls = true;
                        tlsOptions.AllowUntrustedCertificates = true;
                        //tlsOptions.IgnoreCertificateChainErrors = true;
                        //tlsOptions.IgnoreCertificateRevocationErrors = true;

                        if (serverRootCa != null)
                        {
                            tlsOptions.AllowUntrustedCertificates = true;
                            tlsOptions.CertificateValidationCallback += MqttClient_CertificateValidationCallback;
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Setup connection with client and server certificate over TLS" }, LogRecordLevel.Info));
                        }
                        else Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Setup connection with client certificate over TLC" }, LogRecordLevel.Info));

                        optionsBuilder.WithTls(tlsOptions);
                    }

                    _options = optionsBuilder.Build();

                    while (true)
                    {
                        try
                        {
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Connecting to MQTT server ..." }, LogRecordLevel.Info));
                            _mqttClient.ConnectAsync(_options).Wait(new TimeSpan(Timeout));
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Can't connect to MQTT server: {ex.Message}" }, LogRecordLevel.Error));
                            Thread.Sleep(Timeout);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client start was failed: {ex.Message}" }, LogRecordLevel.Error));

            }

            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client started" }, LogRecordLevel.Info));

        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (_mqttClient.IsConnected)
                {
                    try
                    {
                        Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Disconnecting MQTT Client ..." }, LogRecordLevel.Info));

                        if (_mqttClient.DisconnectAsync().Wait(Timeout))
                        {
                            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client disconnect was failed: Timeout" }, LogRecordLevel.Error));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client disconnect was failed: {ex.Message}" }, LogRecordLevel.Error));
                    }
                }
            }
        }

        public bool Publish(string topic, byte[] payload)
        {
            if (!IsConnected) return false;

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithAtLeastOnceQoS()
                .Build();

            _mqttClient.PublishAsync(message);
            return true;
        }

        public bool IsConnected => _mqttClient.IsConnected;
        public string[] Topics { get; private set; }
        public int Timeout { get; set; } = 5000;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<MqttApplicationMessageReceivedEventArgs> MessageReceived;

        private bool MqttClient_CertificateValidationCallback(X509Certificate arg1, X509Chain arg2, SslPolicyErrors arg3, IMqttClientOptions arg4)
        {
            try
            {
                if (arg3 == SslPolicyErrors.None)
                {
                    return true;
                }

                if (arg3.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
                {
                    arg2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    arg2.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    arg2.ChainPolicy.ExtraStore.Add(_serverRootCa);

                    arg2.Build((X509Certificate2)_serverRootCa);
                    var res = arg2.ChainElements.Cast<X509ChainElement>().Any(a => a.Certificate.Thumbprint == _serverRootCa.Thumbprint);
                    return res;
                }
            }
            catch { }

            return false;
        }

        private void MqttClient_Connected(MqttClientConnectedEventArgs e)
        {
            Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Connected to MQTT server: {_brokerUrl}:{_port}" }, LogRecordLevel.Info));

            foreach (string topic in Topics)
            {
                try
                {
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client subscribe to \"{topic}\"..." }, LogRecordLevel.Info));
                    _mqttClient.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce).Wait();
                }
                catch (Exception ex)
                {
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client subscribe to \"{topic}\" fail: {ex.Message}" }, LogRecordLevel.Error));
                }
            }

            Connected?.Invoke(this, EventArgs.Empty);


            //Console.WriteLine("Connected");
            //try
            //{
            //} //ВОт тутт падало, Тк на момент обработки события MQTT уже отвалился
            //catch
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(7));
            //    BackgroundJob.Enqueue(() => MessageProcessor.LogWarning("Error topic subscribing", ""));
            //}

        }

        private void MqttClient_Disconnected(MqttClientDisconnectedEventArgs e)
        {
            if (IsDisposing || IsDisposed)
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client Disconnected: Dispose" }, LogRecordLevel.Info));
                return;
            }

            //  if (e.ClientWasConnected)
            //    return;

            if (e.Exception != null) Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client Disconnected: {e.Exception.Message}" }, LogRecordLevel.Error));
            else
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client Disconnected: No Error" }, LogRecordLevel.Info));
            }
            Disconnected?.Invoke(this, EventArgs.Empty);

            while (true)
            {

                try
                {
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Reconnecting to MQTT server ..." }, LogRecordLevel.Info));
                    Thread.Sleep(Timeout);
                    _mqttClient.ReconnectAsync().Wait(new TimeSpan(Timeout));
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"Error while reconnecting: {ex.Message}" }, LogRecordLevel.Error));
                    Thread.Sleep(Timeout);
                }
            }
            // throw new NotImplementedException();
        }

        private void MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {

                var payload = Encoding.ASCII.GetString(e.ApplicationMessage.Payload);
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client received message. Topic: {e.ApplicationMessage.Topic} Payload: {payload}" }, LogRecordLevel.Debug));

                MessageReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string> { $"MQTT Client received message parse error: {ex.Message}" }, LogRecordLevel.Error));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mqttClient.DisconnectAsync();
                Connected = null;
                Disconnected = null;
                MessageReceived = null;
            }
            base.Dispose(disposing);
        }
    }
}
