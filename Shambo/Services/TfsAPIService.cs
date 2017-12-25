using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Shambo.Model;

namespace Shambo.Services
{
   [Serializable]
   public class TfsAPIService
   {
      public async Task<IEnumerable<Build>> GetBuildsByDefinitionAsync(ConnectionDetails connectionDetails, string buildDefinitionName)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         var buildDefinition = (await buildClient.GetDefinitionsAsync(connectionDetails.TeamProject, name: buildDefinitionName)).SingleOrDefault();

         if (buildDefinition == null)
         {
            return Enumerable.Empty<Build>();
         }

         var builds = (await buildClient.GetBuildsAsync(connectionDetails.TeamProject, new[] { buildDefinition.Id }));

         return builds;
      }

      public async Task<BuildBadge> GetBuildBadgeAsync(ConnectionDetails connectionDetails, Build build)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         try
         {
            var branchName = connectionDetails.RepositoryType == RepoType.TfsVersionControl ? $"$/{connectionDetails.TeamProject}" : "master";

            var buildBadge = await buildClient.GetBuildBadgeAsync(connectionDetails.TeamProject, connectionDetails.RepositoryType.ToString(), branchName: branchName);
            buildBadge.BuildId = build.Id;
            return buildBadge;
         }
         catch (Exception ex)
         {
            throw ex;
         }
      }

      public async Task<IEnumerable<Change>> GetAssociatedChangesAsync(ConnectionDetails connectionDetails, int buildID)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         var changes = await buildClient.GetBuildChangesAsync(connectionDetails.TeamProject, buildID);
         return changes;
      }

      private T GetClient<T>(ConnectionDetails connectionDetails) where T : VssHttpClientBase
      {
         var federatedCreds = new VssBasicCredential("", connectionDetails.PersonalAccessToken);
         var creds = new VssCredentials(federatedCreds);

         var connection = new VssConnection(new Uri(connectionDetails.TfsUrl), creds);
         var buildClient = connection.GetClient<T>();

         return buildClient;
      }
   }
}