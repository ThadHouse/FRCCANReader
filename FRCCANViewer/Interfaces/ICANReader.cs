using FRCCANViewer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRCCANViewer.Interfaces
{
    public interface ICANReader
    {
        /// <summary>
        /// This is not called on the UI thread, must be marshalled correctly.
        /// </summary>
        event EventHandler<CANMessage> CANMessageReceived;
    }
}
