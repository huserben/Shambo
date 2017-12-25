using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Shambo.Dialogs
{
   [Serializable]
   public class HelpDialog : IDialog<object>
   {
      public async Task StartAsync(IDialogContext context)
      {
         var stringBuilder = new StringBuilder();
         stringBuilder.Append($"Here's what I can do for you:{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"• help: displays this help{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"• reset: resets the gathered user data{Environment.NewLine}{Environment.NewLine}");

         await context.PostAsync(stringBuilder.ToString());

         context.Done(context.Activity);
      }
   }
}