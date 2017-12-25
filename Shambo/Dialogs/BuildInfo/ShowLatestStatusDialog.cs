using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Shambo.Model;
using Shambo.Services;

namespace Shambo.Dialogs.BuildInfo
{
   [Serializable]
   public class ShowLatestStatusDialog : IDialog<object>
   {
      private readonly TfsAPIService tfsAPIService;
      private readonly ConnectionDetails connectionDetails;
      private int buildIdToCheck;

      public ShowLatestStatusDialog(TfsAPIService tfsAPIService, ConnectionDetails connectionDetails)
      {
         this.tfsAPIService = tfsAPIService;
         this.connectionDetails = connectionDetails;
      }

      public async Task StartAsync(IDialogContext context)
      {
         await context.PostAsync("Which Build Definition should I check for you?");
         context.Wait(AfterBuildDefinitionWasEntered);
      }


      public async Task AfterBuildDefinitionWasEntered(IDialogContext context, IAwaitable<object> result)
      {
         var activity = (Activity)await result;

         var builds = await tfsAPIService.GetBuildsByDefinitionAsync(connectionDetails, activity.Text);

         var buildToCheck = builds.Where(b => b.Status == BuildStatus.Completed).OrderByDescending(b => b.FinishTime).FirstOrDefault();
         if (buildToCheck != null)
         {
            var badge = await tfsAPIService.GetBuildBadgeAsync(connectionDetails, buildToCheck);

            buildIdToCheck = buildToCheck.Id;
            var webLink = ((ReferenceLink)buildToCheck.Links.Links.Single(l => l.Key == "web").Value).Href;

            var cardButton = new CardAction
            {
               Value = webLink,
               Type = "openUrl",
               Title = buildToCheck.BuildNumber
            };

            var url = new Uri(webLink);
            var baseUrl = webLink.Replace(url.PathAndQuery, "");
            var buildBadgeUrl = $"{baseUrl}/_apis/public/build/definitions/{buildToCheck.Project.Id.ToString()}/{buildToCheck.Definition.Id}/badge";
            var cardImage = new CardImage(buildBadgeUrl, buildToCheck.Result.ToString(), cardButton);

            var heroCard = new HeroCard()
            {
               Title = $"Build Info: {buildToCheck.BuildNumber}",
               Subtitle = $"State: {buildToCheck.Result.ToString()}",
               Buttons = new[] { cardButton },
               Images = new[] { cardImage }
            };

            var attachment = heroCard.ToAttachment();
            var message = context.MakeMessage();
            message.Attachments.Add(attachment);
            message.Text = $"The status of the last {activity.Text} build is: {buildToCheck.Result.ToString()}";

            await context.PostAsync(message);

            PromptDialog.Confirm(
               context,
               AfterPromptForMoreBuildInfo,
               "Do you want to get more infos on this build?");
         }
         else
         {
            PromptDialog.Confirm(
               context,
               AfterPromptForTryingAgain,
               "Could not find a build with that definition, maybe you mispelled it?\n\n Do you want to try again?");
         }
      }

      private async Task AfterPromptForTryingAgain(IDialogContext context, IAwaitable<bool> awaitable)
      {
         var result = await awaitable;

         if (result)
         {
            buildIdToCheck = -1;
            await StartAsync(context);
         }
         else
         {
            context.Done(result);
         }
      }

      private async Task AfterPromptForMoreBuildInfo(IDialogContext context, IAwaitable<bool> awaitable)
      {
         var result = await awaitable;

         if (result)
         {
            context.Call(new BuildInfoDialog(buildIdToCheck, tfsAPIService, connectionDetails), AfterBuildInfoDialogShown);
         }
         else
         {
            context.Done(result);
         }
      }

      private Task AfterBuildInfoDialogShown(IDialogContext context, IAwaitable<object> result)
      {
         context.Done(result);
         return Task.CompletedTask;
      }
   }
}