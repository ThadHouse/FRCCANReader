using System;
using System.Collections.Generic;
using System.Text;

namespace FRCCANViewer.CAN
{
    public class CANInterface
    {
        public string Name { get; set; }
        public string NativeId { get; set; }
        public override string ToString()
        {
            return $"Name: {Name} Path: {NativeId}";
        }
    }
}
