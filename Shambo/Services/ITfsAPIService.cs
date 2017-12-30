using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Shambo.Model;

namespace Shambo.Services
{
   public interface ITfsAPIService
   {
      Task<IEnumerable<string>> GetBuildDefinitionsAsync(ConnectionDetails connectionDetails);

      Task<IEnumerable<Change>> GetAssociatedChangesAsync(ConnectionDetails connectionDetails, int buildID);
      Task<BuildBadge> GetBuildBadgeAsync(ConnectionDetails connectionDetails, Build build);
      Task<Build> GetBuildById(ConnectionDetails connectionDetails, int buildId);
      Task<IEnumerable<Build>> GetBuildsByDefinitionAsync(ConnectionDetails connectionDetails, string buildDefinitionName);
   }
}