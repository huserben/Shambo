using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Shambo.Dialogs
{
   [Serializable]
   public class WelcomeDialog : IDialog<object>
   {
      public async Task StartAsync(IDialogContext context)
      {
         await context.PostAsync("Hello, I'm Shambo. I think we haven't met before. I'm  here to inform you about the state of builds in TFS/VSTS.");

         /*Extend here with more meaningful welcome...*/
         context.UserData.SetValue("hasUsedToolBefore", true);

         context.Done(context.Activity);
      }      
   }
}