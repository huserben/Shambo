using System;
using System.Collections.Generic;
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
         /*Validation if URL exists (ping) and PAT is correct?*/
         return new FormBuilder<Subscription>()
            .Message("Setup your subscription")
            .AddRemainingFields(new[] { nameof(Id), nameof(ConversationReference) })
            .Build();
      }
   }
}