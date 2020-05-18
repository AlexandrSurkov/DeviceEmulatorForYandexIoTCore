using System;
using System.Globalization;
using Flekosoft.Common;
using Flekosoft.Emulation.Iot.Common;

namespace Flekosoft.Emulation.Iot.EnvironmentalSensor
{
    public class HumidityDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {
        readonly Random _rnd = new Random((int) DateTime.Now.Ticks);
        private float _value = 80.5f;

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
            var delta = _rnd.NextDouble();
            if (_rnd.Next(-10, 10) < 0) delta = -delta;

            _value += (float)delta;
            if (_value < 0) _value = 0;

            return _value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class TemperatureDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {
        readonly Random _rnd = new Random((int)DateTime.Now.Ticks);
        private float _value = 25.5f;

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
            var delta = _rnd.NextDouble();
            if (_rnd.Next(-10, 10) < 0) delta = -delta;

            _value += (float)delta;
            if (_value < 0) _value = 0;

            return _value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class PressureDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {
        readonly Random _rnd = new Random((int)DateTime.Now.Ticks);
        private float _value = 20.5f;

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
            var delta = _rnd.NextDouble();
            if (_rnd.Next(-10, 10) < 0) delta = -delta;

            _value += (float)delta;
            if (_value < 0) _value = 0;

            return _value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class CarbonDioxideDataSource : PropertyChangedErrorNotifyDisposableBase, IDataSource
    {
        readonly Random _rnd = new Random((int)DateTime.Now.Ticks);
        private float _value = 123.456f;

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
            var delta = _rnd.NextDouble();
            if (_rnd.Next(-10, 10) < 0) delta = -delta;

            _value += (float)delta;
            if (_value < 0) _value = 0;

            return _value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
