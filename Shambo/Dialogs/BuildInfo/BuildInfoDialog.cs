using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.TeamFoundation.Build.WebApi;
using Shambo.Model;
using Shambo.Services;

namespace Shambo.Dialogs.BuildInfo
{
   [Serializable]
   public class BuildInfoDialog : IDialog<object>
   {
      private readonly int buildId;
      private readonly TfsAPIService tfsApiService;
      private readonly ConnectionDetails connectionDetails;

      public BuildInfoDialog(int buildId, TfsAPIService tfsApiService, ConnectionDetails connectionDetails)
      {
         this.buildId = buildId;
         this.tfsApiService = tfsApiService;
         this.connectionDetails = connectionDetails;
      }

      public Task StartAsync(IDialogContext context)
      {
         PromptDialog.Choice(
            context,
            AfterChoiceMade,
            new List<string> { "Get Associated Changes", "Get Requester", "Back" },
            "What info do you wanna see?");

         return Task.CompletedTask;
      }

      private async Task AfterChoiceMade(IDialogContext context, IAwaitable<string> selectedChoice)
      {
         var choice = await selectedChoice;

         switch (choice)
         {
            case "Get Associated Changes":
               await GetAssociatedChanges(context);
               break;
            case "Get Requester":
               break;
            case "Back":
               context.Done(selectedChoice);
               break;
            default:
               await StartAsync(context);
               break;
         }
      }

      private async Task GetAssociatedChanges(IDialogContext context)
      {
         var changes = await tfsApiService.GetAssociatedChangesAsync(connectionDetails, buildId);

         if (!changes.Any())
         {
            await context.PostAsync("No changes associated with this build.");
            await StartAsync(context);
         }
         else
         {
            var message = context.MakeMessage();

            foreach (var change in changes)
            {
               AddChangeInfoToMessage(change, message);
            }

            message.Text = "Following Changes were associated with the build";
            await context.PostAsync(message);
         }
      }

      private void AddChangeInfoToMessage(Change change, IMessageActivity message)
      {
         var cardImage = new CardImage(change.Author.ImageUrl, change.Author.DisplayName);

         var heroCard = new HeroCard
         {
            Title = change.Message,
            Subtitle = change.Author.DisplayName,
            Images = new[] { cardImage }
         };

         message.Attachments.Add(heroCard.ToAttachment());
      }
   }
}