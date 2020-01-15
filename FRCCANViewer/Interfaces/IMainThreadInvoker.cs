using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRCCANViewer.Interfaces
{
    public interface IMainThreadInvoker
    {
        Task InvokeOnMainThread<T>(Action<T> toInvoke, T data);
        Task InvokeOnMainThread(Action toInvoke);
        void PostOnMainThread(Action toInvoke);
    }
}
