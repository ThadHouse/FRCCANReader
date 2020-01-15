using Avalonia;
using Avalonia.Threading;
using FRCCANViewer.Interfaces;
using FRCCANViewer.Models;
using FRCCANViewer.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FRCCANViewer.ViewModels
{
    public delegate void NewCANMessage(uint id, ulong data, byte dlc, uint ts);

    public class MainWindowViewModel
    {
        public ObservableCollection<CANMessage> CANMessages { get; } = new ObservableCollection<CANMessage>();

        private readonly ICANReader canReader;
        private readonly IMainThreadInvoker mainThreadInvoker;
        private readonly IDependencyInjection di;
        private readonly Action messageReceived;

        public MainWindowViewModel(ICANReader canReader, IMainThreadInvoker mainThreadInvoker, IDependencyInjection di)
        {
            this.di = di;
            this.canReader = canReader;
            this.mainThreadInvoker = mainThreadInvoker;
            this.messageReceived = HandleCANQueue;
            this.canReader.CANMessageReceived += CanReader_CANMessageReceived_RemoteThread;
        }

        private unsafe void CANMessageReceived_UI(uint id, ulong data, byte dlc, uint ts)
        {
            ReadOnlySpan<byte> sData = new ReadOnlySpan<byte>(&data, dlc);

            var msg = CANMessages.Where(x => x.rawId == id).FirstOrDefault();
            if (msg != null)
            {
                // Update message
                msg.UpdateData(sData, ts);

            }
            else
            {
                CANMessages.Add(new CANMessage(id, sData, ts));
            }
        }

        private void HandleCANQueue()
        {
            while (canMessageQueue.TryDequeue(out var result))
            {

                CANMessageReceived_UI(result.id, result.data, result.dlc, result.ts);
            }
        }

        ConcurrentQueue<(uint id, ulong data, byte dlc, uint ts)> canMessageQueue = new ConcurrentQueue<(uint id, ulong data, byte dlc, uint ts)>();

        private void CanReader_CANMessageReceived_RemoteThread(uint id, ulong data, byte dlc, uint ts)
        {
            canMessageQueue.Enqueue((id, data, dlc, ts));
            mainThreadInvoker.PostOnMainThread(messageReceived);
        }

        public void ArbIdCalcWindow()
        {
            ArbitrationCalculator arbWindow = new ArbitrationCalculator();
            arbWindow.Show();
        }

        public void ConfigureCAN()
        {
            DeviceSelector selector = new DeviceSelector(di);
            selector.Show();
        }
    }
}
