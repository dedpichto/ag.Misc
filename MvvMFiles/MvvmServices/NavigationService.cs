using Microsoft.Extensions.DependencyInjection;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ShayCommon.Mvvm.Services
{
    class NavigationService : INavigationService
    {
        private readonly Dictionary<Type, Type> _viewModelToViewMap = new();
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterViewMapping(Type viewModel, Type view)
        {
            _viewModelToViewMap[viewModel] = view;
        }

        public bool? ShowDialogWithOwner<TViewModel, TOwnerViewModel>(object parameters = null) where TViewModel : class where TOwnerViewModel : class
        {
            var viewModelType = typeof(TOwnerViewModel);
            if (!_viewModelToViewMap.TryGetValue(viewModelType, out var viewType))
                return null;
            var view = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.GetType() == viewType);
            var (window, viewModel) = createWindowWithViewModel<TViewModel>(parameters);
            if (view != null)
                window.Owner = view;
            return window?.ShowDialog();
        }

        public TResult ShowDialogWithOwner<TViewModel, TOwnerViewModel, TResult>(object parameters = null) where TViewModel : class where TOwnerViewModel : class
        {
            var viewModelType = typeof(TOwnerViewModel);
            if (!_viewModelToViewMap.TryGetValue(viewModelType, out var viewType))
                return default;
            var view = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.GetType() == viewType);
            var (window, viewModel) = createWindowWithViewModel<TViewModel>(parameters);

            if (window == null) return default;

            if (view != null)
                window.Owner = view;
            var result = window?.ShowDialog();

            if (viewModel is IDialogResult<object> dialogResult)
            {
                return (TResult)dialogResult.Result;
            }
            else if (viewModel is IDialogResult<TResult> resultT)
            {
                return resultT.Result;
            }
            return default;
        }

        public bool? ShowDialog<TViewModel>(object parameters = null) where TViewModel : class
        {
            var (window, viewModel) = createWindowWithViewModel<TViewModel>(parameters);
            return window?.ShowDialog();
        }

        public TResult ShowDialog<TViewModel, TResult>(object parameters = null) where TViewModel : class
        {
            var (window, viewModel) = createWindowWithViewModel<TViewModel>(parameters);

            if (window == null) return default;

            var result = window.ShowDialog();

            if (viewModel is IDialogResult<object> dialogResult)
            {
                return (TResult)dialogResult.Result;
            }
            else if (viewModel is IDialogResult<TResult> resultT)
            {
                return resultT.Result;
            }

            return default;
        }

        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>(object parameters = null) where TViewModel : class
        {
            var (window, viewModel) = createWindowWithViewModel<TViewModel>(parameters);

            if (window == null) return default;

            var tcs = new TaskCompletionSource<TResult>();

            if (viewModel is IDialogResultAsync<TResult> asyncDialogResult)
            {
                asyncDialogResult.ResultCompleted += (sender, result) =>
                {
                    tcs.SetResult(result);
                    window.Close();
                };

                asyncDialogResult.ResultCancelled += (sender, args) =>
                {
                    tcs.SetResult(default);
                    window.Close();
                };
            }

            window.Closed += (sender, args) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(default);
            };

            window.ShowDialog();
            return await tcs.Task;
        }

        public void ShowWindow<TViewModel>(object parameters = null, PropertyComparer propertyComparer = null, bool forceNew = false, bool? updatePermitted = null, bool? updatePermittedRtns = null) where TViewModel : class
        {
            var (window, firstTime) = createWindow<TViewModel>(parameters, propertyComparer, forceNew, updatePermitted, updatePermittedRtns);
            if (window != null)
            {
                if (firstTime)
                {
                    window.Show();
                    window.Dispatcher.InvokeAsync(() => activateWindow(window), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                else
                {
                    if (window.WindowState == WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    activateWindow(window);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        private static void activateWindow(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                window.Activate();
                return;
            }

            var foregroundWindow = GetForegroundWindow();
            var currentThreadId = GetCurrentThreadId();
            var foregroundThreadId = foregroundWindow != IntPtr.Zero
                ? GetWindowThreadProcessId(foregroundWindow, out _)
                : currentThreadId;

            if (currentThreadId != foregroundThreadId)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, true);
                BringWindowToTop(handle);
                SetForegroundWindow(handle);
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }
            else
            {
                BringWindowToTop(handle);
                SetForegroundWindow(handle);
            }

            window.Activate();
        }

        public void ShowModal<TViewModel>(object parameters = null) where TViewModel : class
        {
            var (window, _) = createWindowWithViewModel<TViewModel>(parameters);
            if (window != null)
            {
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Show();
            }
        }

        public TViewModel GetViewModel<TViewModel>() where TViewModel : class
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext is TViewModel);
            if (window != null)
                return window.DataContext as TViewModel;
            return null;
        }

        private (Window, bool) createWindow<TViewModel>(object parameters, PropertyComparer propertyComparer = null, bool forceNew = false, bool? updatePermitted = null, bool? updatePermittedRtns = null) where TViewModel : class
        {
            var viewModelType = typeof(TViewModel);
            if (!_viewModelToViewMap.TryGetValue(viewModelType, out var viewType))
                return (null, false);

            TViewModel viewModel = null;

            var view = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.GetType() == viewType);
            var views = Application.Current.Windows.OfType<Window>().Where(w => w.GetType() == viewType);

            foreach (var v in views)
            {
                if (v.DataContext is TViewModel vm)
                {
                    if (propertyComparer != null)
                    {
                        var props = vm.GetType().GetProperties();
                        var prp = props.FirstOrDefault(p => p.Name == propertyComparer.PropertyName);
                        if (prp != null)
                        {
                            var value = prp.GetValue(vm);
                            if (Equals(value, propertyComparer.PropertyValue))
                            {
                                var type = vm.GetType();
                                if (type == propertyComparer.ViewModel)
                                {
                                    if (vm is IParametersReceiver pReceiver)
                                    {
                                        pReceiver.ReceiveParameters(parameters);
                                    }
                                }
                                return (v, false);
                            }
                        }
                    }
                }
            }

            if (view != null && !forceNew)
            {
                if (view != null)
                {
                    viewModel = view.DataContext as TViewModel;
                }
                if (viewModel is IParametersReceiver pReceiver)
                {
                    pReceiver.ReceiveParameters(parameters);
                }
                return (view, false);
            }

            view = _serviceProvider.GetService(viewType) as Window
                ?? ActivatorUtilities.CreateInstance(_serviceProvider, viewType) as Window;
            if (view != null)
            {
                viewModel = view.DataContext as TViewModel;
            }

            if (viewModel is IParametersReceiver paramReceiver)
            {
                paramReceiver.ReceiveParameters(parameters);
            }

            if (updatePermitted != null)
            {
                var props = viewModel.GetType().GetProperties();
                var prp = props.FirstOrDefault(p => p.Name == "UpdatePermitted");
                if (prp != null)
                {
                    prp.SetValue(viewModel, updatePermitted.Value);
                }
            }
            if (updatePermittedRtns != null)
            {
                var props = viewModel.GetType().GetProperties();
                var prp = props.FirstOrDefault(p => p.Name == "UpdatePermittedRtns");
                if (prp != null)
                {
                    prp.SetValue(viewModel, updatePermittedRtns.Value);
                }
            }
            return (view, true);
        }

        private (Window window, TViewModel viewModel) createWindowWithViewModel<TViewModel>(object parameters) where TViewModel : class
        {
            var viewModelType = typeof(TViewModel);
            if (!_viewModelToViewMap.TryGetValue(viewModelType, out var viewType))
                return (null, null);
            var view = _serviceProvider.GetService(viewType) as Window
                ?? ActivatorUtilities.CreateInstance(_serviceProvider, viewType) as Window;

            TViewModel viewModel = null;
            if (view != null)
            {
                viewModel = view.DataContext as TViewModel;
            }

            if (viewModel is IParametersReceiver paramReceiver)
            {
                paramReceiver.ReceiveParameters(parameters);
            }

            if (viewModel is IDialogViewModel dialogViewModel)
            {
                dialogViewModel.RequestClose += (sender, args) =>
                {
                    view.DialogResult = args.DialogResult;
                    view.Close();
                };
            }
            else if (viewModel is IDialogViewModelWithActions dialogViewModelWithAction)
            {
                dialogViewModelWithAction.RequestClose += (sender, args) =>
                {
                    view.DialogResult = args.DialogResult;
                    view.Close();
                };
            }
            return (view, viewModel);
        }

        public bool CloseWindow<TView>() where TView : Window
        {
            var view = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is TView);
            if (view != null)
            {
                view.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<TViewModel> GetViewModels<TViewModel>() where TViewModel : class
            => Application.Current.Windows.OfType<Window>().Where(w => w.DataContext is TViewModel).Select(w => w.DataContext as TViewModel);
    }

    public class PropertyComparer
    {
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public Type ViewModel { get; set; }
    }
}
