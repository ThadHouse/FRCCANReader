using Avalonia.Threading;
using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FRCCANViewer.ViewModels
{
    public class MainWindowViewModel
    {
        public ObservableCollection<CANMessage> CANMessages { get; } = new ObservableCollection<CANMessage>();

        private readonly ICANReader canReader;
        private readonly IMainThreadInvoker mainThreadInvoker;
        private readonly Action<CANMessage> messageReceived;

        public MainWindowViewModel(ICANReader canReader, IMainThreadInvoker mainThreadInvoker)
        {
            this.canReader = canReader;
            this.mainThreadInvoker = mainThreadInvoker;
            this.messageReceived = CANMessageReceived_UI;
            this.canReader.CANMessageReceived += CanReader_CANMessageReceived_RemoteThread;
        }

        private void CANMessageReceived_UI(CANMessage e)
        {

        }

        private void CanReader_CANMessageReceived_RemoteThread(object sender, CANMessage e)
        {
            mainThreadInvoker.InvokeOnMainThread(messageReceived, e);
        }
    }
}
