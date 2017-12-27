using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shambo.Model;
using Shambo.Model.Botdata;
using Shambo.Models;

namespace Shambo.Services
{
   public class EntityFrameworkDataService : IDataService
   {
      private readonly BotConfigContext db = new BotConfigContext();

      private readonly BotConfiguration botConfig;

      public EntityFrameworkDataService()
      {
         if (!db.BotConfigurations.Any())
         {
            botConfig = new BotConfiguration
            {
               ConnectionDetails = new ConnectionDetails(),
               Subscriptions = new List<Subscription>()
            };

            db.BotConfigurations.Add(botConfig);
            db.SaveChanges();
         }
         else
         {
            botConfig = db.BotConfigurations
               .Include(x => x.Subscriptions)
               .Include(x => x.ConnectionDetails)
            .Single();
         }
      }

      public ConnectionDetails GetConnectionDetails()
      {
         return botConfig.ConnectionDetails;
      }

      public IEnumerable<Subscription> GetSubscriptions()
      {
         return botConfig.Subscriptions;
      }

      public void StoreConnectionDetails(ConnectionDetails connectionDetails)
      {
         db.Entry(botConfig.ConnectionDetails).State = EntityState.Modified;
         botConfig.ConnectionDetails = connectionDetails;
         db.SaveChanges();
      }

      public void AddSubscription(Subscription newSubscription)
      {
         db.Entry(newSubscription).State = EntityState.Added;
         botConfig.Subscriptions.Add(newSubscription);
         db.SaveChanges();
      }
   }
}