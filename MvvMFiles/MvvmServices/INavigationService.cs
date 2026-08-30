using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ShayCommon.Mvvm.Services
{
    public interface INavigationService
    {
        void RegisterViewMapping(Type viewModel, Type view);
        bool? ShowDialog<TViewModel>(object parameters = null) where TViewModel : class;
        TResult ShowDialog<TViewModel, TResult>(object parameters = null) where TViewModel : class;
        bool? ShowDialogWithOwner<TViewModel, TOwnerViewModel>(object parameters = null) where TViewModel : class where TOwnerViewModel : class;
        TResult ShowDialogWithOwner<TViewModel, TOwnerViewModel, TResult>(object parameters = null) where TViewModel : class where TOwnerViewModel : class;
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(object parameters = null) where TViewModel : class;
        void ShowWindow<TViewModel>(object parameters = null, PropertyComparer propertyComparer = null, bool forceNew = false, bool? updatePermitted = null, bool? updatePermittedRtns = null) where TViewModel : class;
        void ShowModal<TViewModel>(object parameters = null) where TViewModel : class;
        bool CloseWindow<TView>() where TView : Window;
        TViewModel GetViewModel<TViewModel>() where TViewModel : class;
        IEnumerable<TViewModel> GetViewModels<TViewModel>() where TViewModel : class;
    }
}
