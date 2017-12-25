using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Shambo.Model;
using Shambo.Model.WebHooks;

namespace Shambo.Dialogs.Notifications
{
   [Serializable]
   public class BuildEventNotificationDialog : IDialog<Activity>
   {
      private readonly BuildCompletedEvent buildCompletedEvent;
      private readonly Subscription subscription;

      public BuildEventNotificationDialog(BuildCompletedEvent buildCompletedEvent, Subscription subscription)
      {
         this.buildCompletedEvent = buildCompletedEvent;
         this.subscription = subscription;
      }

      public async Task StartAsync(IDialogContext context)
      {
         /*
         var buildDefintition = buildCompletedEvent.Resource.Definition.Name;
         var buildId = buildCompletedEvent.Resource.BuildNumber;
         var result = buildCompletedEvent.Resource.Status;
         var lastChangedBy = buildCompletedEvent.Resource.LastChangedBy.DisplayName;
         */

         await context.PostAsync("Heey man, got a notification for you!");

         var messageActivity = (Activity)context.Activity;
         context.Done(messageActivity);
      }
   }
}