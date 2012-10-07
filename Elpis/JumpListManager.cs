using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Shell;
using PandoraSharp;

namespace Elpis
{
    public static class JumpListManager
    {
        public static JumpTask createJumpTask(string title, string description, string commandArg, int iconIndex)
        {
            var task = new JumpTask();
            task.Title = title;
            task.Description = description;
            task.ApplicationPath = Assembly.GetEntryAssembly().Location;
            task.Arguments = commandArg;
            task.IconResourcePath = task.ApplicationPath;
            task.IconResourceIndex = iconIndex;
            return task;
        }

        public static JumpTask createJumpTask(RoutedUICommand command, string commandArg, int iconIndex)
        {
            return createJumpTask(command.Name,command.Text,commandArg,iconIndex);
        }
    }
}
