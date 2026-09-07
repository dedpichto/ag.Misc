using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Windows.Documents;

namespace ShayCommon.Mvvm.Helpers
{
    public class FlowDocumentBehavior : Behavior<FlowDocument>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += onFlowDocumentLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= onFlowDocumentLoaded;
        }

        private void onFlowDocumentLoaded(object sender, EventArgs e)
        {
            var viewModel = AssociatedObject.DataContext as BaseViewModelWithActions;
            if (viewModel != null && AssociatedObject.Tag is Enum extTag)
            {
                viewModel.RegisterGetFlowDocumentFunction(extTag, () =>
                {
                    return AssociatedObject;
                });
            }
            else if (viewModel != null && AssociatedObject.Tag is Guid guid)
            {
                viewModel.RegisterGetFlowDocumentFunction(guid, () =>
                {
                    return AssociatedObject;
                });
            }
        }
    }
}
