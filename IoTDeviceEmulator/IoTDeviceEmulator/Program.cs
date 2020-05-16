using System;
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
        static void Main(string[] args)
        {
            Logger.Instance.LoggerOutputs.Add(Logger.ConsoleOutput);

            var rootCaCrt = new X509Certificate2(X509Certificate.CreateFromSignedFile("rootCA.crt"));

            var deviceEmulator = new DeviceEmulator(
                new IDataSource[]
                {
                    new HumidityDataSource(),
                    new CarbonDioxideDataSource(),
                    new PressureDataSource(),
                    new TemperatureDataSource()
                },
                new YandexCloudCommunicationInterface("are570ke057oir85l9fr", "brRapM4pT8JScMPb", String.Empty, String.Empty, rootCaCrt)
                );


            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
