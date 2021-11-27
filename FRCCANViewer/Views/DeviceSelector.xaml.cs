using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml;
using FRCCANViewer.Interfaces;
using FRCCANViewer.ViewModels;

namespace FRCCANViewer.Views
{
    public class DeviceSelector : Window
    {
        public DeviceSelector(IDependencyInjection di)
        {
            var vm = di.Resolve<DeviceSelectorViewModel>();
            DataContext = vm;
            this.InitializeComponent();
            vm.Refresh();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public DeviceSelector()
        {

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
