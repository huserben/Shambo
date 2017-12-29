using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.TeamFoundation.Build.WebApi;
using Shambo.Model;
using Shambo.Services;

namespace Shambo.Dialogs.BuildInfo
{
   [Serializable]
   public class CheckBuildInfoDialog : IDialog<object>
   {
      private readonly ConnectionDetails connectionDetails;
      private readonly ITfsAPIService tfsApiService;
      private readonly string buildName;
      private readonly string buildStatus;
      private readonly int numberOfBuilds;

      public CheckBuildInfoDialog(ConnectionDetails connectionDetails, ITfsAPIService tfsApiService, string buildName, string buildStatus, int numberOfBuilds)
      {
         this.connectionDetails = connectionDetails;
         this.tfsApiService = tfsApiService;
         this.buildName = buildName;
         this.buildStatus = buildStatus;
         this.numberOfBuilds = numberOfBuilds;
      }

      public async Task StartAsync(IDialogContext context)
      {
         var builds = await GetBuildsByDefinitionName();

         if (!builds.Any())
         {
            await context.PostAsync($"Could not find any builds for the specified definition \"{buildName}\"");
            context.Done((object)null);
         }
         else
         {
            builds = FilterBuildsByStatus(builds);

            if (!builds.Any())
            {
               await context.PostAsync($"Could not find any builds with the status \"{buildStatus}\"");
               context.Done((object)null);
            }
            else
            {
               builds = builds.Take(numberOfBuilds);

               if (builds.Count() < numberOfBuilds)
               {
                  await context.PostAsync($"Could not find more than {builds.Count()} builds - will display them");
               }

               context.Call(new ShowBuildStatusDialog(tfsApiService, connectionDetails, builds.Select(b => b.Id).ToList()), AfterBuildInfoCheckWasExecuted);
            }
         }
      }

      private async Task<IEnumerable<Build>> GetBuildsByDefinitionName()
      {
         IEnumerable<Build> builds = null;

         if (string.IsNullOrEmpty(buildName) || buildName == "any")
         {
            /*We don't know the build definition name - let's try for all the build definitions then...*/
            builds = (await tfsApiService.GetBuildsByDefinitionAsync(connectionDetails, string.Empty))
               .OrderByDescending(b => b.FinishTime);
         }
         else
         {
            // We know the build definition, so let's filter by name.
            builds = (await tfsApiService.GetBuildsByDefinitionAsync(connectionDetails, buildName))
               .OrderByDescending(b => b.FinishTime);
         }

         return builds;
      }

      private IEnumerable<Build> FilterBuildsByStatus(IEnumerable<Build> builds)
      {
         switch (buildStatus)
         {
            case "Successful":
               builds = builds.Where(b => b.Status == BuildStatus.Completed && b.Result == BuildResult.Succeeded);
               break;
            case "Failing":
               builds = builds.Where(b => b.Status == BuildStatus.Completed && b.Result == BuildResult.Failed);
               break;
            case "Running":
               builds = builds.Where(b => b.Status == BuildStatus.InProgress);
               break;
            default:
               // If we don't have a status, just check the completed ones...
               builds = builds.Where(b => b.Status == BuildStatus.Completed);
               break;
         }

         return builds;
      }

      private Task AfterBuildInfoCheckWasExecuted(IDialogContext context, IAwaitable<object> result)
      {
         context.Done((object)null);
         return Task.CompletedTask;
      }
   }
}