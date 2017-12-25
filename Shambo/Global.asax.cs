using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Shambo.Services;

namespace Shambo
{
   public class WebApiApplication : HttpApplication
   {
      protected void Application_Start()
      {
         GlobalConfiguration.Configure(WebApiConfig.Register);

         var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
         Conversation.UpdateContainer(
           builder =>
           {
              builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();

              builder.Register(c => new CachingBotDataStore(store,
                         CachingBotDataStoreConsistencyPolicy
                         .ETagBasedConsistency))
                         .As<IBotDataStore<BotData>>()
                         .AsSelf()
                         .InstancePerLifetimeScope();
              
              builder.RegisterType<EntityFrameworkDataService>().As<IDataService>().SingleInstance();
           });
      }
   }
}
