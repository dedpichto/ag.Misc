using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace ShayCommon.Mvvm.Services
{
    class WindowBridgeServiceFactory : IWindowBridgeServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowBridgeServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IWindowBridgeService Create(Window window)
        {
            var bridgeService = (WindowBridgeService)_serviceProvider.GetService<IWindowBridgeService>();
            bridgeService.AttachWindow(window);
            return bridgeService;
        }
    }
}
