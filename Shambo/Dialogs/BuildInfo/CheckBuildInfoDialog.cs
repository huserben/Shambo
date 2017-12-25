using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Shambo.Model;
using Shambo.Services;

namespace Shambo.Dialogs.BuildInfo
{
   [Serializable]
   public class CheckBuildInfoDialog : IDialog<object>
   {
      private readonly TfsAPIService tfsAPiService;
      private readonly ConnectionDetails connectionDetails;

      public CheckBuildInfoDialog(ConnectionDetails connectionDetails)
      {
         tfsAPiService = new TfsAPIService();
         this.connectionDetails = connectionDetails;
      }

      public async Task StartAsync(IDialogContext context)
      {
         await DisplayDialogHelp(context);
      }

      private Task DisplayDialogHelp(IDialogContext context)
      {
         var promptOptions = new List<string> { "showLatestStatus", "getBuilds", "isRunning", "help", "back" };

         PromptDialog.Choice(
            context,
            MessageReceived,
            promptOptions,
            "What can I do for you?");

         return Task.CompletedTask;
      }

      private async Task MessageReceived(IDialogContext context, IAwaitable<string> result)
      {
         var text = await result;

         switch (text)
         {
            case "showLatestStatus":
               context.Call(new ShowLatestStatusDialog(tfsAPiService, connectionDetails), AfterBuildInfoCheckWasExecuted);
               break;
            case "getBuilds":
               /* TODO */
               break;
            case "isRunning":
               /* TODO */
               break;
            case "exit":
               context.Done(result);
               break;
            case "help":
            default:
               await DisplayDialogHelp(context);
               break;
         }
      }

      private Task AfterBuildInfoCheckWasExecuted(IDialogContext context, IAwaitable<object> result)
      {
         PromptDialog.Confirm(
            context,
            AfterCheckIfShouldContinue,
            "Do you want to fetch more build infos?");

         return Task.CompletedTask;
      }

      private async Task AfterCheckIfShouldContinue(IDialogContext context, IAwaitable<bool> awaitableResult)
      {
         var result = await awaitableResult;

         if (result)
         {
            await DisplayDialogHelp(context);
         }
         else
         {
            context.Done(result);
         }
      }
   }
}