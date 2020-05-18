using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Flekosoft.Common.Logging;
using Flekosoft.Emulation.Iot.Common;
using Flekosoft.Emulation.Iot.Communication.YandexCloud;
using Flekosoft.Emulation.Iot.EnvironmentalSensor;

namespace IoTDeviceEmulator
{
    class Program
    {

        /// <param name="deviceID">Yandex.Cloud IoTCore DeviceID</param>
        /// <param name="password">Yandex.Cloud IoTCore Device password</param>
        static void Main(string deviceID, string password)
        {
            Logger.Instance.LoggerOutputs.Add(Logger.ConsoleOutput);

            if (string.IsNullOrEmpty(deviceID))
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string>() { "deviceID can't be empty" }, LogRecordLevel.Error));
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                Logger.Instance.AppendLog(new LogRecord(DateTime.Now, new List<string>() { "password can't be empty" }, LogRecordLevel.Error));
                return;
            }

            var rootCaCrt = new X509Certificate2(X509Certificate.CreateFromSignedFile("rootCA.crt"));

            var deviceEmulator = new DeviceEmulator(Guid.NewGuid().ToString(),
                new IDataSource[]
                {
                    new HumidityDataSource(),
                    new CarbonDioxideDataSource(),
                    new PressureDataSource(),
                    new TemperatureDataSource()
                },
                new YandexCloudCommunicationInterface(deviceID, password, String.Empty, String.Empty, rootCaCrt)
                );


            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
