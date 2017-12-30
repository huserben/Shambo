using System;
using System.Text;
using Microsoft.Bot.Builder.FormFlow;

namespace Shambo.Model
{
   [Serializable]
   public class Subscription
   {
      public int Id { get; set; }

      public string ConversationReference { get; set; }

      [Prompt("Comma-separated list of build definitions that should be subscribed to (* if you want it for every build definition)")]
      public string BuildDefinitionNames { get; set; }

      [Prompt("Comma-separated list of build results that should trigger the notification? (Any, Succeeded, PartiallySucceeded, Failed, Canceled)")]
      public string BuildStates { get; set; }

      public static IForm<Subscription> BuildForm()
      {
         var stringBuilder = new StringBuilder();
         stringBuilder.Append($"I will notify you as soon as one of the following builds has completed and it's in the one of the states you set: " +
            $"{Environment.NewLine}{Environment.NewLine}");

         stringBuilder.Append($"Build Definitions: {{{nameof(BuildDefinitionNames)}}} {Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"States: {{{nameof(BuildStates)}}} {Environment.NewLine}{Environment.NewLine}");

         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append("Is your configuration ok like this?");

         return new FormBuilder<Subscription>()
            .Message("Ok, let me setup a subscription for you")
            .Field(nameof(BuildDefinitionNames))
            .Field(nameof(BuildStates))
            .Confirm(stringBuilder.ToString())
            .Build();
      }
   }
}