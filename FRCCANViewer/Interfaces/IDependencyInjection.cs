using Autofac;

namespace FRCCANViewer.Interfaces
{
    public interface IDependencyInjection
    {
        IContainer Container { get; }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
