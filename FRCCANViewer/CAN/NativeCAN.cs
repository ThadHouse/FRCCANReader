using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using FRCCANViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FRCCANViewer.CAN
{

    /*
     * 
     * #pragma once

#include <stdint.h>

struct CAN_Device {
    char* deviceId;
    char* name;
};

struct CANData {
  uint64_t timestamp;
  int32_t id;
  uint8_t length;
  uint8_t data[8];
};


extern "C" {
    struct CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length);
    void FRC_CAN_Reader_Native_FreeDevices(struct CAN_Device* devices, int length);

    void FRC_CAN_Reader_Native_Start();
    void FRC_CAN_Reader_Native_SetDevice(const char* deviceId);

    int FRC_CAN_Reader_Native_ReadMessage(struct CANData* data);
    void FRC_CAN_Reader_Native_ReleaseMessage();
}


    */

    public unsafe class NativeCAN : ICANReader, ICANInterfaceManager
    {
        public event NewCANMessage CANMessageReceived;

        private readonly Thread readThread;

        private struct CANData
        {
            public ulong timestamp;
            public int id;
            public byte length;
            public ulong data;
        };

        private struct CAN_Device
        {
            public byte* deviceId;
            public byte* name;
        };


        public NativeCAN()
        {
            FRC_CAN_Reader_Native_Start();

            readThread = new Thread(() =>
            {
                CANData data;
                while (true)
                {
                    if (FRC_CAN_Reader_Native_ReadMessage(&data) == 0)
                    {
                        continue;
                    }
                    CANMessageReceived?.Invoke((uint)data.id, data.data, data.length, (uint)data.timestamp);
                }
            });
            readThread.IsBackground = true;
            readThread.Name = "CAN Managed Reader";
            readThread.Start();
        }

        public void SetInterface(CANInterface can)
        {
            if (can == null) return;
            byte* address = stackalloc byte[can.NativeId.Length + 1];
            Encoding.ASCII.GetBytes(can.NativeId, new Span<byte>(address, can.NativeId.Length + 1));
            address[can.NativeId.Length] = 0;
            FRC_CAN_Reader_Native_SetDevice(address);
        }

        public CANInterface[] GetInterfaces()
        {
            int length = 0;
            var devices = FRC_CAN_Reader_Native_EnumerateDevices(&length);

            var ret = new CANInterface[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new CANInterface
                {
                    Name = Marshal.PtrToStringAnsi((IntPtr)devices[i].name),
                    NativeId = Marshal.PtrToStringAnsi((IntPtr)devices[i].deviceId)
                };
            }

            FRC_CAN_Reader_Native_FreeDevices(devices, length);

            return ret;
        }

        [DllImport("FRC_CAN_Reader_Native", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FRC_CAN_Reader_Native_Start();

        [DllImport("FRC_CAN_Reader_Native", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int FRC_CAN_Reader_Native_ReadMessage(CANData* data);

        [DllImport("FRC_CAN_Reader_Native", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length);

        [DllImport("FRC_CAN_Reader_Native", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void FRC_CAN_Reader_Native_FreeDevices(CAN_Device* devices, int length);

        [DllImport("FRC_CAN_Reader_Native", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void FRC_CAN_Reader_Native_SetDevice(byte* deviceId);
    }
}
