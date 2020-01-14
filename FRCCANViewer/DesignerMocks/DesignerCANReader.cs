using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRCCANViewer.DesignerMocks
{
    public class DesignerCANReader : ICANReader
    {
        public event EventHandler<CANMessage> CANMessageReceived;
    }
}
