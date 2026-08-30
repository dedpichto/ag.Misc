using ShayCommon.Mvvm.ViewModels;
using System;

namespace ShayCommon.Mvvm.Commands
{
    public static class MvvmCommands
    {
        public static void InitiaLizeSharedCommand(IUICommand command, Action<IUICommand> executeAction)
        {
            command.Bind(
                (obj) => executeAction(command),
                (obj) => true);
        }
        public static void InitializeCommand(IUICommand command, IViewModel context)
        {
            command.Bind(
                (obj) => context.CommandExecuted(command),
                (obj) => context.CommandCanExecute(command));
            context.Commands.Add(command);
        }
        public static void InitializeCommand(IUICommand command, IDialogViewModel context)
        {
            command.Bind(
                (obj) => context.CommandExecuted(command),
                (obj) => context.CommandCanExecute(command));
            context.Commands.Add(command);
        }
    }
}
