using System;

namespace Flekosoft.Emulation.Iot.Common
{
    public interface IDataSource : IDisposable
    {
        string GetDataName();
        string GetValueType();
        string GetValue();
    }
}
