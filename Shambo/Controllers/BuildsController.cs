using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Shambo.Dialogs.Notifications;
using Shambo.Model.WebHooks;
using Shambo.Services;

namespace Shambo.Controllers
{
   public class BuildsController : ApiController
   {
      private readonly IDataService dataService;
      private readonly ITfsAPIService tfsApiService;

      public BuildsController()
      {
         dataService = Conversation.Container.Resolve<IDataService>();
         tfsApiService = Conversation.Container.Resolve<ITfsAPIService>();
      }

      public async Task<HttpResponseMessage> Post([FromBody]BuildCompletedEvent buildCompletedEvent)
      {
         var subscriptions = dataService.GetSubscriptions();

         foreach (var subscription in subscriptions)
         {
            if (!ShouldNotify(buildCompletedEvent, subscription))
            {
               continue;
            }

            var message = JsonConvert.DeserializeObject<ConversationReference>(subscription.ConversationReference).GetPostToUserMessage();
            var client = new ConnectorClient(new Uri(message.ServiceUrl));
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
               var botData = scope.Resolve<IBotData>();
               await botData.LoadAsync(CancellationToken.None);

               // This is the dialog stack.
               var task = scope.Resolve<IDialogTask>();
               var stack = scope.Resolve<IDialogStack>();

               if (stack.Frames.Count > 0)
               {
                  // Create the new dialog and add it to the stack.
                  var dialog = new BuildEventNotificationDialog(buildCompletedEvent);
                  task.Call(dialog.Void<object, IMessageActivity>(), null);
                  await task.PollAsync(CancellationToken.None);

                  // Flush the dialog stack back to its state store.
                  await botData.FlushAsync(CancellationToken.None);
               }
            }
         }

         var response = Request.CreateResponse(HttpStatusCode.OK);
         return response;
      }

      private bool ShouldNotify(BuildCompletedEvent buildCompletedEvent, Model.Subscription subscription)
      {
         var buildDefinition = buildCompletedEvent.Resource.Definition.Name;
         var result = buildCompletedEvent.Resource.Status;

         if (subscription.BuildDefinitionName != "*" && !subscription.BuildDefinitionName.Contains(buildDefinition))
         {
            return false;
         }

         if (subscription.BuildState != "Any" && !subscription.BuildState.Select(x => x.ToString()).Contains(result))
         {
            return false;
         }

         return true;
      }
   }
}