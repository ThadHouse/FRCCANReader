using FRCCANViewer.Models;
using FRCCANViewer.ViewModels;
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
        event NewCANMessage CANMessageReceived;
    }
}
