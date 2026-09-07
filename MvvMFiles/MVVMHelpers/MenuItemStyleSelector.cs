using System.Windows;
using System.Windows.Controls;

namespace ShayCommon.Mvvm.Helpers
{
    public class MenuItemStyleSelector : StyleSelector
    {
        public Style MenuItemStyle { get; set; }
        public Style SeparatorStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is MenuSeparatorViewModel)
                return SeparatorStyle;
            else if (item is MenuItemViewModel)
                return MenuItemStyle;
            return base.SelectStyle(item, container);
        }
    }
}
