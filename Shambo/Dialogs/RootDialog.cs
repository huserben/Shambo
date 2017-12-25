using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Shambo.Dialogs.BuildInfo;
using Shambo.Helpers;
using Shambo.Model;

namespace Shambo.Dialogs
{
   [Serializable]
   public class RootDialog : IDialog<object>
   {
      public async Task StartAsync(IDialogContext context)
      {
         if (!context.UserData.TryGetValue<bool>("hasUsedToolBefore", out _))
         {
            context.Call(new WelcomeDialog(), AfterWelcomeDialog);
         }
         else
         {
            await CheckConfiguration(context);
         }
      }
      
      private async Task AfterWelcomeDialog(IDialogContext context, IAwaitable<object> result)
      {
         await CheckConfiguration(context);
      }

      private async Task CheckConfiguration(IDialogContext context)
      {
         var connectionDetails = Conversation.Container.GetDataService().GetConnectionDetails();
         if (string.IsNullOrEmpty(connectionDetails.TfsUrl))
         {
            await context.PostAsync("You did not set up any connection detail yet. You cannot continue without this.");
            context.Call(FormDialog.FromForm(ConnectionDetails.BuildForm), AfterConnectionSetup);
         }
         else
         {
            await OfferServices(context, null);
         }
      }

      private async Task AfterConnectionSetup(IDialogContext context, IAwaitable<ConnectionDetails> formResult)
      {
         var connectionDetails = await formResult;

         if (connectionDetails != null)
         {
            Conversation.Container.GetDataService().StoreConnectionDetails(connectionDetails);

            await OfferServices(context, null);
         }
         else
         {
            await CheckConfiguration(context);
         }
      }

      private async Task AfterSubscriptionSetup(IDialogContext context, IAwaitable<Subscription> formResult)
      {
         var newSubscription = await formResult;

         if (newSubscription != null)
         {
            var message = context.MakeMessage();
            var conversationReference = message.ToConversationReference();
            newSubscription.ConversationReference = JsonConvert.SerializeObject(conversationReference);

            Conversation.Container.GetDataService().AddSubscription(newSubscription);
         }
      }

      private async Task OfferServices(IDialogContext context, IAwaitable<object> result)
      {
         await context.PostAsync($"Hi how can I be of service?");

         context.Wait(ConversationStarted);
      }

      private async Task ConversationStarted(IDialogContext context, IAwaitable<object> incomingActivity)
      {
         var activity = (Activity)await incomingActivity;

         /* Handle with LUIS */
         switch (activity.Text.ToLower())
         {
            case "modfiy connection":
               context.Call(FormDialog.FromForm(ConnectionDetails.BuildForm), AfterConnectionSetup);
               break;
            case "add subscription":
               context.Call(FormDialog.FromForm(Subscription.BuildForm), AfterSubscriptionSetup);
               break;
            case "remove subscription":
               /*TODO*/
               break;
            case "check build state":
               context.Call(new CheckBuildInfoDialog(Conversation.Container.GetDataService().GetConnectionDetails()), AfterHelpDialog);
               break;
            case "reset":
               context.Call(new ResetUserDataDialog(), AfterResetUserData);
               break;
            case "help":
            default:
               context.Call(new HelpDialog(), AfterHelpDialog);
               break;
         }
      }

      private Task AfterHelpDialog(IDialogContext context, IAwaitable<object> result)
      {
         context.Wait(ConversationStarted);
         return Task.CompletedTask;
      }

      private async Task AfterResetUserData(IDialogContext context, IAwaitable<object> result)
      {
         await context.PostAsync("User data has been deleted, time to make a fresh start.");
         await StartAsync(context);
      }
   }
}