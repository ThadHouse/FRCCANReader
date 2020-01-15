using FRCCANViewer.CAN;
using FRCCANViewer.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FRCCANViewer.ViewModels
{
    public class DeviceSelectorViewModel : INotifyPropertyChanged
    {
        private readonly ICANInterfaceManager canManager;

        public DeviceSelectorViewModel(ICANInterfaceManager canManager)
        {
            this.canManager = canManager;
        }

        private CANInterface selectedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CANInterface SelectedValue
        {
            get => selectedValue;
            set
            {
                canManager.SetInterface(value);
                selectedValue = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<CANInterface> Devices { get; } = new ObservableCollection<CANInterface>();

        public void Refresh()
        {
            Devices.Clear();
            foreach (var d in canManager.GetInterfaces())
            {
                Devices.Add(d);
            }
            if (Devices.Count > 0)
            {
                SelectedValue = Devices[0];
            }
            else
            {
                SelectedValue = null;
            }
        }
    }
}
