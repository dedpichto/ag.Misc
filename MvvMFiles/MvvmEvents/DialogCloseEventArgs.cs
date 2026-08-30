using System;

namespace ShayCommon.Mvvm.Events
{
    public class DialogCloseEventArgs : EventArgs
    {
        public bool? DialogResult { get; set; }
        public DialogCloseEventArgs(bool? dialogResult = null) => DialogResult = dialogResult;
    }
}
