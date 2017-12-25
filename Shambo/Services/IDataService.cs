using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Shambo.Model;

namespace Shambo.Services
{
   public interface IDataService
   {
      ConnectionDetails GetConnectionDetails();

      void StoreConnectionDetails(ConnectionDetails connectionDetails);

      void AddSubscription(Subscription newSubscription);

      IEnumerable<Subscription> GetSubscriptions();
   }
}