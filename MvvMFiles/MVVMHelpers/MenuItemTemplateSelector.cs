using System.Windows;
using System.Windows.Controls;

namespace ShayCommon.Mvvm.Helpers
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MenuItemTemplate { get; set; }
        public DataTemplate SeparatorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MenuSeparatorViewModel)
                return SeparatorTemplate;
            else if (item is MenuItemViewModel)
                return MenuItemTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
