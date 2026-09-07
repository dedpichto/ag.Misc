using BLL.WPF.GroupsChooser;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public class GroupsChooserSelectionChangedBehavior : Behavior<GroupsChooser>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(GroupsChooserSelectionChangedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedGroupsChanged += onSelectorSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectedGroupsChanged -= onSelectorSelectionChanged;
        }
        private void onSelectorSelectionChanged(object sender, SelectedGroupsChangedEventArgs e)
        {
            if (AssociatedObject is GroupsChooser)
            {
                if (Command != null && Command.CanExecute(e))
                {
                    Command.Execute(e);
                }
            }
        }
    }

    public class GroupsChooserHelper : Behavior<GroupsChooser>
    {
        private RoutedEventHandler _loadedHandler;

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler=async(sender,e) => await Dispatcher.InvokeAsync(() =>
            {
                var viewModel = AssociatedObject.DataContext as BaseViewModelWithActionShay;
                if (viewModel == null)
                {
                    var rootView = findRootView(AssociatedObject);
                    if (rootView != null)
                        viewModel = rootView.DataContext as BaseViewModelWithActionShay;
                }
                if (viewModel != null && AssociatedObject.Tag is Enum extTag)
                {
                    viewModel.RegisterSelectGroupsAction(extTag, (groups) =>
                    {
                        AssociatedObject.SelectGroups(groups);
                    });
                    viewModel.RegisterClearGroupsAction(extTag, () =>
                    {
                        AssociatedObject.Clear();
                    });
                    viewModel.RegisterGetSelectedGroupMembersFunction(extTag, () =>
                    {
                        return AssociatedObject.SelectedMembers;
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterAddGroupsAction(guid, (groups) =>
                    {
                        AssociatedObject.SelectGroups(groups);
                    });
                    viewModel.RegisterClearGroupsAction(guid, () =>
                    {
                        AssociatedObject.Clear();
                    });
                    viewModel.RegisterGetSelectedGroupMembersFunction(guid, () =>
                    {
                        return AssociatedObject.SelectedMembers;
                    });
                }
            }, DispatcherPriority.DataBind);
            AssociatedObject.Loaded += _loadedHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_loadedHandler != null)
                AssociatedObject.Loaded -= _loadedHandler;
        }

        private static FrameworkElement findRootView(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Window || element is UserControl)
                    return element as FrameworkElement;

                if (element is FrameworkElement frameworkElement && frameworkElement.TemplatedParent != null)
                {
                    element = frameworkElement.TemplatedParent;
                }
                else
                {
                    element = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);
                }
            }
            return null;
        }
    }
}
