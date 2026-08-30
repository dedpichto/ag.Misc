using System.Windows.Input;

namespace ShayCommon.Mvvm.Commands
{
    public class CommandGesture
    {
        public string Name { get; set; }
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }
    }
}
