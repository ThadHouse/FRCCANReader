using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRCCANViewer.ViewModels;

namespace FRCCANViewer.Views
{
    public class ArbitrationCalculator : Window
    {
        public ArbitrationCalculator()
        {
            DataContext = new ArbitrationCalculatorViewModel();
            this.InitializeComponent();
            this.Width = 600;
            this.Height = 400;
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
