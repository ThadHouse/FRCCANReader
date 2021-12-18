using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FRCCANViewer.Models
{
    public enum DeviceType
    {
        kBroadcast = 0,
        kRobotController = 1,
        kMotorController = 2,
        kRelayController = 3,
        kGyroSensor = 4,
        kAccelerometer = 5,
        kUltrasonicSensor = 6,
        kGearToothSensor = 7,
        kPowerDistribution = 8,
        kPneumatics = 9,
        kMiscellaneous = 10,
        kFirmwareUpdate = 31
    }

    public enum Manufacturer
    {
        kBroadcast = 0,
        kNI = 1,
        kLM = 2,
        kDEKA = 3,
        kCTRE = 4,
        kREV = 5,
        kGrapple = 6,
        kMS = 7,
        kTeamUse = 8,
        kKauaiLabs = 9,
        kCopperforge = 10,
        kPWF = 11,
        kStudica = 12
    }

    public class CANMessage : INotifyPropertyChanged
    {
        public readonly uint rawId;

        public event PropertyChangedEventHandler PropertyChanged;

        //public bool Extended => (rawId & (uint)IdMasks.Extended) != 0;
        //public bool RTR => (rawId & (uint)IdMasks.RTR) != 0;
        public uint Id => rawId & 0x1FFFFFFF;
        public DeviceType DeviceType => (DeviceType)((rawId >> 24) & 0x1F);
        public Manufacturer Manufacturer => (Manufacturer)((rawId >> 16) & 0xFF);
        public uint ApiId => (rawId >> 6) & 0x3FF;
        public uint DeviceId => rawId & 0x3F;
        private readonly byte[] data = new byte[8];

        public Memory<byte> Data => data.AsMemory().Slice(0, (int)DataLength);
        public uint DataLength { get; private set; }
        public uint TimeStamp { get; private set; }

        public uint Delta { get; private set; }

        unsafe struct CircularBuffer
        {
            const int count = 16;
            private fixed uint values[count];
            private int index = 0;

            public uint Average
            {
                get
                {
                    ulong sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        sum += values[i];
                    }
                    return (uint)(sum / count);
                }
            }

            public void Insert(uint value)
            {
                values[index] = value;
                index++;
                if (index >= count)
                {
                    index = 0;
                }
            }
        }

        CircularBuffer DeltaBuffer;

        uint lastPrintTime = 0;

        public uint SmoothDelta => DeltaBuffer.Average;

        public CANMessage(uint rawId, ReadOnlySpan<byte> data, uint ts)
        {
            this.rawId = rawId;
            DataLength = (uint)data.Length;
            data.CopyTo(this.data);
            TimeStamp = ts;
        }

        public void UpdateData(ReadOnlySpan<byte> data, uint ts)
        {
            DataLength = (uint)data.Length;
            data.CopyTo(this.data);
            Delta = ts - TimeStamp;
            DeltaBuffer.Insert(Delta);
            TimeStamp = ts;
            RaisePropertyChanged(nameof(TimeStamp));
            RaisePropertyChanged(nameof(Data));
            RaisePropertyChanged(nameof(DataLength));
            RaisePropertyChanged(nameof(Delta));
            uint elapsed = ts - lastPrintTime;
            if (elapsed >= TimeSpan.FromMilliseconds(500).TotalMilliseconds * 1000) {
                RaisePropertyChanged(nameof(SmoothDelta));
                lastPrintTime = ts;
            }
        }

        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
