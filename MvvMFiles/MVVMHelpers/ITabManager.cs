using System;
using System.Collections.Generic;

namespace ShayCommon.Mvvm.Helpers
{
    public interface ITabManager
    {
        event EventHandler TabOrderChanged;
        event EventHandler TabStopChanged;

        int GetTabIndex(string controlName);
        bool? GetIsTabStop(string controlName);

        void SetTabOrder(params string[] controlNames);
        void SetTabStops(Dictionary<string, bool> tabStops);
        void EnableTabStop(params string[] controlNames);
        void DisableTabStop(params string[] controlNames);
    }
}
