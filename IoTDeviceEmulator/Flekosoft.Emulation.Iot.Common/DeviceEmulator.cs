using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;
using Flekosoft.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flekosoft.Emulation.Iot.Common
{
    public class DeviceEmulator : PropertyChangedErrorNotifyDisposableBase
    {
        readonly List<IDataSource> _dataSources = new List<IDataSource>();

        private Timer _sendDataTimer;
        private readonly object _timerSyncObject = new object();
        private int _sendDataInterval;
        private readonly ICommunicationInterface _communicationInterface;

        public DeviceEmulator(IDataSource[] dataSources, ICommunicationInterface communicationInterface)
        {
            _communicationInterface = communicationInterface;
            _communicationInterface.NewDataFromServerEvent += _communicationInterface_NewDataFromServerEvent;
            _dataSources.AddRange(dataSources);
            SendDataIntervalMs = 1000;


        }

        private void _communicationInterface_NewDataFromServerEvent(object sender, JsonStringEventArgs e)
        {
            ParseDataFromServer(e.JsonString);
        }

        private void _sendDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                dynamic data = new JObject();
                data.TimeStamp = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
                var list = new List<JObject>();
                foreach (IDataSource source in _dataSources)
                {
                    dynamic value = new JObject();
                    value.Type = source.GetValueType();
                    value.Name = source.GetDataName();
                    value.Value = source.GetValue();
                    list.Add(value);
                }
                data.Values = new JArray(list);

                var jsonString = JsonConvert.SerializeObject(data);

                _communicationInterface.SendJsonData(jsonString);
            }
            catch (Exception ex)
            {
                AppendExceptionLogMessage(ex);
            }
            
        }

        protected void ParseDataFromServer(string jsonData)
        {

        }

        private void RestartTimer()
        {
            if (IsDisposed) return;
            lock (_timerSyncObject)
            {
                if (_sendDataTimer != null)
                {
                    _sendDataTimer.Stop();
                    _sendDataTimer.Dispose();
                    _sendDataTimer = null;
                }
                if (SendDataIntervalMs != 0)
                {
                    _sendDataTimer = new System.Timers.Timer(SendDataIntervalMs);
                    _sendDataTimer.Elapsed += _sendDataTimer_Elapsed;
                    _sendDataTimer.Start();
                }
            }
        }

        

        public int SendDataIntervalMs
        {
            get => _sendDataInterval;
            set
            {
                if (_sendDataInterval != value)
                {
                    lock (_timerSyncObject)
                    {
                        _sendDataInterval = value;
                        RestartTimer();
                        OnPropertyChanged(nameof(SendDataIntervalMs));
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_timerSyncObject)
                {
                    if (_sendDataTimer != null)
                    {
                        _sendDataTimer.Stop();
                        _sendDataTimer.Dispose();
                        _sendDataTimer = null;
                    }
                }

                foreach (IDataSource dataSource in _dataSources)
                {
                    dataSource.Dispose();
                }

                _communicationInterface.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
