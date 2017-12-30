using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Shambo.Model;

namespace Shambo.Services
{
   [Serializable]
   public class TfsAPIService : ITfsAPIService
   {
      public async Task<IEnumerable<Build>> GetBuildsByDefinitionAsync(ConnectionDetails connectionDetails, string buildDefinitionName)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         IEnumerable<BuildDefinitionReference> definitionsToCheck = await buildClient.GetDefinitionsAsync(project: connectionDetails.TeamProject);

         if (!string.IsNullOrEmpty(buildDefinitionName))
         {
            definitionsToCheck = definitionsToCheck.Where(d => d.Name.ToLower().Contains(buildDefinitionName.ToLower()));
         }

         if (!definitionsToCheck.Any())
         {
            return Enumerable.Empty<Build>();
         }

         var builds = new List<Build>();

         foreach (var definition in definitionsToCheck)
         {
            var buildsForDefinition = (await buildClient.GetBuildsAsync(connectionDetails.TeamProject, new[] { definition.Id }));
            builds.AddRange(buildsForDefinition);
         }

         return builds;
      }

      public async Task<Build> GetBuildById(ConnectionDetails connectionDetails, int buildId)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         var build = await buildClient.GetBuildAsync(buildId);
         return build;
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

      public async Task<IEnumerable<string>> GetBuildDefinitionsAsync(ConnectionDetails connectionDetails)
      {
         var buildClient = GetClient<BuildHttpClient>(connectionDetails);

         IEnumerable<BuildDefinitionReference> definitionsToCheck = await buildClient.GetDefinitionsAsync(project: connectionDetails.TeamProject);

         return definitionsToCheck.Select(x => x.Name).ToList();
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