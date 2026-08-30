using System.Windows;

namespace ShayCommon.Mvvm.Services
{
    public interface IWindowBridgeServiceFactory
    {
        IWindowBridgeService Create(Window window);
    }
}
