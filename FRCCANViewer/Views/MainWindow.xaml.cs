using Autofac;
using Autofac.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using FRCCANViewer.CAN;
using FRCCANViewer.Converters;
using FRCCANViewer.DesignerMocks;
using FRCCANViewer.Interfaces;
using FRCCANViewer.Utilities;
using FRCCANViewer.ViewModels;

namespace FRCCANViewer.Views
{

    public class MainWindow : Window, IDependencyInjection
    {
        public MainWindow()
        {
            // 
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(this).AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(MainWindow).Assembly).Where(x => x.Name.EndsWith("ViewModel")).SingleInstance().AsSelf().AsImplementedInterfaces();

            if (Design.IsDesignMode)
            {
                builder.RegisterType<DesignerCANReader>().SingleInstance().AsImplementedInterfaces();
                builder.RegisterType<DesignerMainThreadInvoker>().SingleInstance().AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterType<NativeCAN>().SingleInstance().AsImplementedInterfaces();
                builder.RegisterType<AvaloniaMainThreadInvoker>().SingleInstance().AsImplementedInterfaces();
            }

            Container = builder.Build();

            DataContext = Container.Resolve<MainWindowViewModel>();
            InitializeComponent();

            var grid = this.Get<DataGrid>("DataGrid");
            grid.AutoGeneratingColumn += Grid_AutoGeneratingColumn;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public IContainer Container { get; }

        private void Grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id" || e.PropertyName == "TimeStamp")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = HexidecimalConverter.EightCharHexConverter;
            }
            else if (e.PropertyName == "ApiId")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = HexidecimalConverter.ThreeCharHexConverter;
            }
            else if (e.PropertyName == "Data")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = MemoryConverter.Singleton;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
