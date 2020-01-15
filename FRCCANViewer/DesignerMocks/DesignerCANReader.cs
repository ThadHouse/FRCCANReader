using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using FRCCANViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRCCANViewer.DesignerMocks
{
    public class DesignerCANReader : ICANReader
    {
        public event NewCANMessage CANMessageReceived;
    }
}
