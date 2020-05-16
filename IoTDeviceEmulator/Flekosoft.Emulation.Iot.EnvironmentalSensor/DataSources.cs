using System;
using System.Globalization;
using Flekosoft.Common;
using Flekosoft.Emulation.Iot.Common;

namespace Flekosoft.Emulation.Iot.EnvironmentalSensor
{
    public class HumidityDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {

        public string GetDataName()
        {
            return "Humidity";
        }

        public string GetValueType()
        {
            return "Float";
        }

        public string GetValue()
        {
            return 80.5f.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class TemperatureDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {

        public string GetDataName()
        {
            return "Temperature";
        }

        public string GetValueType()
        {
            return "Float";
        }

        public string GetValue()
        {
            return 25.5f.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class PressureDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {

        public string GetDataName()
        {
            return "Pressure";
        }

        public string GetValueType()
        {
            return "Float";
        }

        public string GetValue()
        {
            return 20.5f.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class CarbonDioxideDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {

        public string GetDataName()
        {
            return "CarbonDioxide";
        }

        public string GetValueType()
        {
            return "Float";
        }

        public string GetValue()
        {
            return 123.456f.ToString(CultureInfo.InvariantCulture);
        }
    }
}
