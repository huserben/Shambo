using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.TeamFoundation.Build.WebApi;
using Shambo.Helpers;

namespace Shambo.Model
{
   [Serializable]
   public class Subscription
   {
      private static Task<IEnumerable<string>> possibleBuildDefinitionsTask;

      public Subscription()
      {
         possibleBuildDefinitionsTask = null;
      }

      public int Id { get; set; }

      public string ConversationReference { get; set; }

      public string BuildDefinitionName { get; set; }

      [Optional]
      public string BuildState { get; set; }

      public static IEnumerable<string> PossibleBuildStates
      {
         get
         {
            var states = new List<string> { string.Empty };
            states.AddRange(Enum.GetNames(typeof(BuildResult)));
            return states;
         }
      }

      public static Task<IEnumerable<string>> PossibleBuildDefinitions
      {
         get
         {
            if (possibleBuildDefinitionsTask == null)
            {
               possibleBuildDefinitionsTask = Conversation.Container.GetTfsApiService().GetBuildDefinitionsAsync(Conversation.Container.GetDataService().GetConnectionDetails());
            }

            return possibleBuildDefinitionsTask;
         }
      }

      public static IForm<Subscription> BuildForm()
      {
         var stringBuilder = new StringBuilder();
         stringBuilder.Append($"I will notify you as soon as a {{{nameof(BuildDefinitionName)}}} build result is {{{nameof(BuildState)}}}" +
            $"{Environment.NewLine}{Environment.NewLine}");

         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append("Is your configuration ok like this?");

         return new FormBuilder<Subscription>()
            .Message("Ok, let me setup a subscription for you")
            .Field(new FieldReflector<Subscription>(nameof(BuildDefinitionName))
               .SetType(null)
               .SetDefine(async (state, field) =>
               {
                  var possibleBuildDefinitions = await PossibleBuildDefinitions;
                  foreach (var buildDefinition in possibleBuildDefinitions)
                  {
                     field.AddDescription(buildDefinition, buildDefinition)
                     .AddTerms(buildDefinition, buildDefinition);
                  }

                  return true;
               }))
            .Field(new FieldReflector<Subscription>(nameof(BuildState))
               .SetType(null)
               .SetDefine((state, field) =>
               {
                  foreach (var possibleResult in PossibleBuildStates)
                  {
                     field.AddDescription(possibleResult, possibleResult)
                     .AddTerms(possibleResult, possibleResult);
                  }

                  return Task.FromResult(true);
               }))
            .Confirm(stringBuilder.ToString())
            .Build();
      }
   }
}