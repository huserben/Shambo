using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Shambo.Dialogs.BuildInfo;
using Shambo.Helpers;
using Shambo.Model;
using Shambo.Model.WebHooks;

namespace Shambo.Dialogs.Notifications
{
   [Serializable]
   public class BuildEventNotificationDialog : IDialog<Activity>
   {
      private readonly BuildCompletedEvent buildCompletedEvent;

      public BuildEventNotificationDialog(BuildCompletedEvent buildCompletedEvent)
      {
         this.buildCompletedEvent = buildCompletedEvent;
      }

      public Task StartAsync(IDialogContext context)
      {
         var buildDefintition = buildCompletedEvent.Resource.Definition.Name;
         var result = buildCompletedEvent.Resource.Status;
         var lastChangedBy = buildCompletedEvent.Resource.LastChangedBy.DisplayName;

         PromptDialog.Confirm(
            context,
            AfterPrompt,
            $"A build of {buildDefintition} just finished with the status {result}. You want to see more Details?");

         return Task.CompletedTask;
      }

      private async Task AfterPrompt(IDialogContext context, IAwaitable<bool> result)
      {
         var showMoreDetail = await result;

         if (showMoreDetail)
         {
            var buildStatusDialog = new ShowBuildStatusDialog(
               Conversation.Container.GetTfsApiService(),
               Conversation.Container.GetDataService().GetConnectionDetails(),
               new[] { buildCompletedEvent.Resource.Id });

            context.Call(buildStatusDialog, AfterBuildDetailsShown);
         }
         else
         {
            var messageActivity = (Activity)context.Activity;
            context.Done(messageActivity);
         }
      }

      private Task AfterBuildDetailsShown(IDialogContext context, IAwaitable<object> result)
      {
         var messageActivity = (Activity)context.Activity;
         context.Done(messageActivity);
         return Task.CompletedTask;
      }
   }
}