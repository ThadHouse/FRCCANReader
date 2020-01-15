using FRCCANViewer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRCCANViewer.DesignerMocks
{
    public class DesignerMainThreadInvoker : IMainThreadInvoker
    {
        public Task InvokeOnMainThread<T>(Action<T> toInvoke, T data)
        {
            return Task.CompletedTask;
        }

        public Task InvokeOnMainThread(Action toInvoke)
        {
            return Task.CompletedTask;
        }

        public void PostOnMainThread(Action toInvoke)
        {
            
        }
    }
}
