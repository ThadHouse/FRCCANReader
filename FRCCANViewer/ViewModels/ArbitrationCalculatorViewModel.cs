using FRCCANViewer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace FRCCANViewer.ViewModels
{
    public class ArbitrationCalculatorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaiseAndSetIfChanged<T>(ref T storage, T value, [CallerMemberName] string propName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return;
            }
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string deviceId;
        public string DeviceId
        {
            get => deviceId;
            set => RaiseAndSetIfChanged(ref deviceId, value);
        }

        private string fullArbId;
        public string FullArbId
        {
            get => fullArbId;
            set => RaiseAndSetIfChanged(ref fullArbId, value);
        }

        private string apiId;
        public string ApiId
        {
            get => apiId;
            set => RaiseAndSetIfChanged(ref apiId, value);
        }

        private string manufacturer;
        public string Manufacturer
        {
            get => manufacturer;
            set
            {
                RaiseAndSetIfChanged(ref manufacturer, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManufacturerEnum)));
            }
        }

        private string deviceType;
        public string DeviceType
        {
            get => deviceType;
            set {
                RaiseAndSetIfChanged(ref deviceType, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceTypeEnum)));
            }
        }

        private bool hexidecimal;
        public bool Hexidecimal
        {
            get => hexidecimal;
            set => RaiseAndSetIfChanged(ref hexidecimal, value);
        }

        public DeviceType DeviceTypeEnum
        {
            get
            {
                if (int.TryParse(TrimHex(DeviceType), out var res))
                {
                    return (DeviceType)res;
                }
                return Models.DeviceType.kBroadcast;
            }
        }

        public Manufacturer ManufacturerEnum
        {
            get
            {
                if (int.TryParse(TrimHex(Manufacturer), out var res))
                {
                    return (Manufacturer)res;
                }
                return Models.Manufacturer.kBroadcast;
            }
        }


        public ArbitrationCalculatorViewModel()
        {
            PropertyChanged += ArbitrationCalculatorViewModel_PropertyChanged;
        }

        private uint CreateArbId(ushort apiId, byte deviceId, byte manufacturer, byte deviceType)
        {
                return (((uint)deviceType & 0x1F) << 24 | ((uint)manufacturer & 0xFF) << 16 | ((uint)apiId & 0x3FF) << 6 | ((uint)deviceId & 0x3F));
        }

        private ReadOnlySpan<char> TrimHex(ReadOnlySpan<char> input)
        {
            var lastX = input.LastIndexOf("X", StringComparison.InvariantCultureIgnoreCase);
            if (lastX < 0) return input;
            return input.Slice(lastX + 1);
        }

        private void ArbitrationCalculatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Hexidecimal))
            {

            }
            else if (e.PropertyName == nameof(FullArbId))
            {
                // Compute dev id's
                if (uint.TryParse(TrimHex(FullArbId), NumberStyles.HexNumber, null, out var arbId))
                {
                    byte deviceId = (byte)(arbId & 0x3F);
                    ushort apiId = (ushort)((arbId >> 6) & 0x3FF);
                    byte manufacturer = (byte)((arbId >> 16) & 0xFF);
                    byte deviceType = (byte)((arbId >> 24) & 0x1F);

                    DeviceId = deviceId.ToString("X2");
                    ApiId = apiId.ToString("X3");
                    Manufacturer = manufacturer.ToString("X2");
                    DeviceType = deviceType.ToString("X2");
                }
                
            } 
            else
            {
                // Compute full ID
                if (byte.TryParse(TrimHex(DeviceId), NumberStyles.HexNumber, null, out byte deviceId)
                    && ushort.TryParse(TrimHex(ApiId), NumberStyles.HexNumber, null, out ushort apiId)
                    && byte.TryParse(TrimHex(Manufacturer), NumberStyles.HexNumber, null, out byte manufacturer)
                    && byte.TryParse(TrimHex(DeviceType), NumberStyles.HexNumber, null, out byte deviceType))
                {
                    FullArbId = CreateArbId(apiId, deviceId, manufacturer, deviceType).ToString("X8");
                }
            }
        }
    }
}
