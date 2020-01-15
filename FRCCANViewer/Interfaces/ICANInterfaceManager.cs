using FRCCANViewer.CAN;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRCCANViewer.Interfaces
{
    public interface ICANInterfaceManager
    {
        CANInterface[] GetInterfaces();
        void SetInterface(CANInterface can);
    }
}
