using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Shambo.Helpers;

namespace Shambo.Dialogs
{
   [Serializable]
   public class ResetUserDataDialog : IDialog<object>
   {
      public async Task StartAsync(IDialogContext context)
      {
         await context.PostAsync("Deleting user data...");
         context.UserData.Clear();

         Conversation.Container.GetDataService().ClearData();

         context.Done(context.Activity);
      }
   }
}