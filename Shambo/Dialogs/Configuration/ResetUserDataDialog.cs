using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Shambo.Dialogs
{
   [Serializable]
   public class ResetUserDataDialog : IDialog<object>
   {
      public async Task StartAsync(IDialogContext context)
      {
         await context.PostAsync("Deleting user data...");
         context.UserData.Clear();

         context.Done(context.Activity);
      }
   }
}