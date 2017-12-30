using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using Shambo.Dialogs.BuildInfo;
using Shambo.Helpers;
using Shambo.Model;
using Shambo.Services;

namespace Shambo.Dialogs
{
   [Serializable]
   public class RootDialog : LuisDialog<object>
   {
      private readonly ITfsAPIService tfsAPIService;

      public RootDialog() : base(new LuisService(new LuisModelAttribute(
         ConfigurationManager.AppSettings["LuisAppId"],
         ConfigurationManager.AppSettings["LuisAPIKey"])))
      {
         tfsAPIService = Conversation.Container.GetTfsApiService();
      }

      public override async Task StartAsync(IDialogContext context)
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
            context.Wait(MessageReceived);
         }
      }

      private async Task AfterConnectionSetup(IDialogContext context, IAwaitable<ConnectionDetails> formResult)
      {
         var connectionDetails = await formResult;

         if (connectionDetails != null)
         {
            Conversation.Container.GetDataService().StoreConnectionDetails(connectionDetails);
            await context.PostAsync("Ok thanks, now we're ready to go. How can I help?");
            context.Wait(MessageReceived);
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
            Conversation.Container.GetDataService().AddSubscription(newSubscription);

            await context.PostAsync("Allright, I added it to the configuration.");
         }
      }

      [LuisIntent("Greeting")]
      private async Task AfterGreetingMessageReceived(IDialogContext context, LuisResult result)
      {
         await context.PostAsync("Hello there, how can I be of your service? :-)");
         context.Wait(MessageReceived);
      }

      [LuisIntent("None")]
      [LuisIntent("")]
      private async Task AfterUnknownMessageReceived(IDialogContext context, LuisResult result)
      {
         await context.PostAsync("Sorry, I don't understand what you mean. Wanna try again?");
         context.Wait(MessageReceived);
      }

      [LuisIntent("Help")]
      private Task AfterHelpMessageReceived(IDialogContext context, LuisResult result)
      {
         context.Call(new HelpDialog(), AfterHelpDialog);

         return Task.CompletedTask;
      }

      [LuisIntent("Build.CheckStatus")]
      private Task AfterCheckBuildStateMessageReceived(IDialogContext context, LuisResult result)
      {
         ExtractEntitiesFromLuisResult(result, out var buildName, out var buildState, out var numberOfBuilds);

         context.Call(new CheckBuildInfoDialog(
            Conversation.Container.GetDataService().GetConnectionDetails(),
            Conversation.Container.GetTfsApiService(),
            buildName,
            buildState,
            numberOfBuilds),
            AfterHelpDialog);
         return Task.CompletedTask;
      }

      [LuisIntent("ResetData")]
      private Task AfterResetMessageReceived(IDialogContext context, LuisResult result)
      {
         context.Call(new ResetUserDataDialog(), AfterResetUserData);
         return Task.CompletedTask;
      }

      /*TODO: Add intent*/
      [LuisIntent("Connection.Modify")]
      private Task AfterModifyConnectionMessageReceived(IDialogContext context, LuisResult result)
      {
         context.Call(FormDialog.FromForm(ConnectionDetails.BuildForm), AfterConnectionSetup);
         return Task.CompletedTask;
      }

      [LuisIntent("Subscription.Add")]
      private async Task AfterAddSubscriptionMessageReceived(IDialogContext context, LuisResult result)
      {
         var subscription = new Subscription();
         var message = context.MakeMessage();
         var conversationReference = message.ToConversationReference();
         subscription.ConversationReference = JsonConvert.SerializeObject(conversationReference);

         ExtractEntitiesFromLuisResult(result, out var buildName, out var buildState, out var _);

         var availableBuildDefinitions = await Subscription.PossibleBuildDefinitions;
         var matchingBuildDefinition = availableBuildDefinitions.SingleOrDefault(bd => bd.ToLower() == buildName.ToLower());
         if (matchingBuildDefinition != null)
         {
            subscription.BuildDefinitionName = matchingBuildDefinition;
         }
         
         subscription.BuildState = BuildStateToBuildResult(buildState);

         var formDialog = new FormDialog<Subscription>(subscription, Subscription.BuildForm, FormOptions.PromptInStart);
         context.Call(formDialog, AfterSubscriptionSetup);
      }

      private Task AfterHelpDialog(IDialogContext context, IAwaitable<object> result)
      {
         context.Wait(MessageReceived);
         return Task.CompletedTask;
      }

      private async Task AfterResetUserData(IDialogContext context, IAwaitable<object> result)
      {
         await context.PostAsync("User data has been deleted, time to make a fresh start.");
         await StartAsync(context);
      }

      private void ExtractEntitiesFromLuisResult(LuisResult result, out string buildName, out string buildState, out int numberOfBuilds)
      {
         if (result.TryFindEntity("BuildName", out var buildNameEntity))
         {
            buildName = buildNameEntity.Entity;
            /*Todo: handle "any" etc.*/
         }
         else
         {
            buildName = string.Empty;
         }

         if (result.TryFindEntity("BuildState", out var buildStateEntity))
         {
            var values = (List<object>)buildStateEntity.Resolution["values"];
            buildState = values.Single().ToString();
         }
         else
         {
            buildState = string.Empty;
         }

         if (result.TryFindEntity("builtin.number", out var numberEntity))
         {
            int.TryParse(numberEntity.Entity, out numberOfBuilds);
         }
         else
         {
            numberOfBuilds = 1;
         }
      }

      /// <summary>
      ///  Transforms build state from LUIS to build result as defined by Enum
      /// </summary>
      /// <param name="buildState">Build state fetched from LUIS</param>
      /// <returns>BuildResult enum matching counterpart</returns>
      private string BuildStateToBuildResult(string buildState)
      {
         switch (buildState)
         {
            case "Canceled":
            case "":
               return buildState;
            case "Partially Succeeded":
               return "PartiallySucceeded";
            case "Failing":
               return "Failed";
            case "Successful":
               return "Succeeded";
            default:
               return "None";
         }
      }
   }
}