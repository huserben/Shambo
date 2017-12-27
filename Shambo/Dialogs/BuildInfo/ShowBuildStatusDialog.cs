using System;
using System.Collections.Generic;
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
   public class ShowBuildStatusDialog : IDialog<object>
   {
      private readonly TfsAPIService tfsAPIService;
      private readonly ConnectionDetails connectionDetails;
      private readonly IEnumerable<int> buildIds;
      private int buildIdToCheck;

      public ShowBuildStatusDialog(TfsAPIService tfsAPIService, ConnectionDetails connectionDetails, IEnumerable<int> buildIds)
      {
         this.tfsAPIService = tfsAPIService;
         this.connectionDetails = connectionDetails;
         this.buildIds = buildIds;
      }

      public async Task StartAsync(IDialogContext context)
      {
         foreach (var buildId in buildIds)
         {
            await DisplayBuildInfoAsync(context, buildId);
         }

         buildIdToCheck = buildIds.SingleOrDefault();
         if (buildIdToCheck != 0)
         {
            PromptDialog.Confirm(
               context,
               AfterPromptForMoreBuildInfo,
               "Do you want to get more infos on this build?");
         }
         else
         {
            context.Done((object)null);
         }
      }

      private async Task DisplayBuildInfoAsync(IDialogContext context, int buildId)
      {
         var build = await tfsAPIService.GetBuildById(connectionDetails, buildId);
         var badge = await tfsAPIService.GetBuildBadgeAsync(connectionDetails, build);
         var webLink = ((ReferenceLink)build.Links.Links.Single(l => l.Key == "web").Value).Href;

         var cardButton = new CardAction
         {
            Value = webLink,
            Type = "openUrl",
            Title = build.BuildNumber
         };

         var url = new Uri(webLink);
         var baseUrl = webLink.Replace(url.PathAndQuery, "");
         var buildBadgeUrl = $"{baseUrl}/_apis/public/build/definitions/{build.Project.Id.ToString()}/{build.Definition.Id}/badge";
         var cardImage = new CardImage(buildBadgeUrl, build.Result.ToString(), cardButton);

         var heroCard = new HeroCard()
         {
            Title = $"Build Info: {build.BuildNumber}",
            Subtitle = $"State: {build.Result.ToString()}",
            Buttons = new[] { cardButton },
            Images = new[] { cardImage }
         };

         var attachment = heroCard.ToAttachment();
         var message = context.MakeMessage();
         message.Attachments.Add(attachment);
         message.Text = $"The status of the last {build.Definition.Name} build is: {build.Result.ToString()}";

         await context.PostAsync(message);
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