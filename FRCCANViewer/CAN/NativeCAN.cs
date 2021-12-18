using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using FRCCANViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        const string NativeCANLibrary = "FRC_CAN_Reader_Native";
        public event NewCANMessage CANMessageReceived;

        private readonly Thread readThread;

        private struct CANData
        {
            public ulong timestamp;
            public int id;
            public byte length;
            public fixed byte data[8];
        };

        private struct CAN_Device
        {
            public byte* deviceId;
            public byte* name;
        };

        static NativeCAN()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeCAN).Assembly, ImportResolver);
        }

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == NativeCANLibrary)
            {
                // Get current EXE dir
                var loc = assembly.Location;
                var dirName = Path.GetDirectoryName(loc);
                var libName = Path.Join(dirName, "Natives", "FRC_CAN_Reader_Native");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (sizeof(IntPtr) == 8)
                    {
                        libName += ".dll.windowsx86-64";
                    }
                    else
                    {
                        libName += ".dll.windowsx86";
                    }
                }
                // Try using the system library 'libmylibrary.so.5'
                NativeLibrary.TryLoad(libName, assembly, DllImportSearchPath.System32, out libHandle);
            }
            return libHandle;
        }

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
                    ulong ulongData = Unsafe.ReadUnaligned<ulong>(data.data);
                    CANMessageReceived?.Invoke((uint)data.id, ulongData, data.length, (uint)data.timestamp);
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

        [DllImport(NativeCANLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FRC_CAN_Reader_Native_Start();

        [DllImport(NativeCANLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int FRC_CAN_Reader_Native_ReadMessage(CANData* data);

        [DllImport(NativeCANLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CAN_Device* FRC_CAN_Reader_Native_EnumerateDevices(int* length);

        [DllImport(NativeCANLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void FRC_CAN_Reader_Native_FreeDevices(CAN_Device* devices, int length);

        [DllImport(NativeCANLibrary, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void FRC_CAN_Reader_Native_SetDevice(byte* deviceId);
    }
}
