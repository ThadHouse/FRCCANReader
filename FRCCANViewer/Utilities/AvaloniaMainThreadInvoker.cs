using Avalonia.Threading;
using FRCCANViewer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRCCANViewer.Utilities
{
    public class AvaloniaMainThreadInvoker : IMainThreadInvoker
    {
        public Task InvokeOnMainThread<T>(Action<T> toInvoke, T data)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                toInvoke(data);
            });
        }

        public Task InvokeOnMainThread(Action toInvoke)
        { 
            return Dispatcher.UIThread.InvokeAsync(toInvoke);
        }

        public void PostOnMainThread(Action toInvoke)
        {
            Dispatcher.UIThread.Post(toInvoke);
        }
    }
}
