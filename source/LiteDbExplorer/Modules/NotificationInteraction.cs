using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Enterwell.Clients.Wpf.Notifications;
using LiteDbExplorer.Controls;

namespace LiteDbExplorer.Modules
{
    public class NotificationInteraction
    {
        private static readonly Lazy<NotificationInteraction> _instance =
            new Lazy<NotificationInteraction>(() => new NotificationInteraction());

        private readonly NotificationMessageManager _manager;

        private NotificationInteraction()
        {
            _manager = new NotificationMessageManager();
        }

        public static INotificationMessageManager Manager => _instance.Value._manager;

        public static NotificationMessageBuilder Default()
        {
            var builder = Manager
                .CreateMessage()
                .Animates(true)
                .Accent(StyleKit.PrimaryHueDarkBrush)
                .Background(StyleKit.MaterialDesignCardBackground)
                ;

            builder.SetForeground(StyleKit.MaterialDesignBody);

            return builder;
        }

        public static void Alert(string message, Action closeAction = null)
        {
            Default()
                .HasMessage(message)
                .Dismiss().WithButton("Close", button => { closeAction?.Invoke(); })
                .Queue();
        }

        public static Task<string> ActionsSheet(string message, IDictionary<string, string> actions)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var builder = Default()
                .HasMessage(message);

            foreach (var action in actions)
            {
                if (action.Key.StartsWith(@"-"))
                {
                    builder = builder.WithButton(action.Value, button => { taskCompletionSource.TrySetResult(action.Key); });
                }
                else
                {
                    builder = builder
                        .Dismiss().WithButton(action.Value, button => { taskCompletionSource.TrySetResult(action.Key); });
                }
            }
            
            builder.Queue();

            return taskCompletionSource.Task;
        }
    }

    public class NotificationInteractionProxy : Freezable
    {
        public INotificationMessageManager Manager => NotificationInteraction.Manager;

        protected override Freezable CreateInstanceCore()
        {
            return new NotificationInteractionProxy();
        }
    }
}