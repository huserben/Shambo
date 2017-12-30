using System;
using System.Text;
using Microsoft.Bot.Builder.FormFlow;

namespace Shambo.Model
{
   public enum RepoType
   {
      TfsVersionControl,
      TfsGit
   }

   [Serializable]
   public class ConnectionDetails
   {
      public int Id { get; set; }

      [Prompt("Define URL of TFS/VSTS Instance (including collection)")]
      public string TfsUrl
      {
         get;
         set;
      }

      [Prompt("Which Team Project do you want to access?")]
      public string TeamProject { get; set; }

      [Prompt("Which type of Repository is this? {||}", ChoiceFormat ="{1}")]
      public RepoType? RepositoryType { get; set; }

      [Prompt("Personal Access Toekn to access Server")]
      public string PersonalAccessToken { get; set; }

      public static IForm<ConnectionDetails> BuildForm()
      {
         var stringBuilder = new StringBuilder();
         stringBuilder.Append($"• TfsUrl: {{{nameof(TfsUrl)}}}{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"• TeamProject: {{{nameof(TeamProject)}}}{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"• PersonalAccessToken: {{{nameof(PersonalAccessToken)}}}{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"• RepositoryType: {{{nameof(RepositoryType)}}}{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append($"{Environment.NewLine}{Environment.NewLine}");
         stringBuilder.Append("Is your configuration ok like this?");

         /*Validation if URL exists (ping) and PAT is correct?*/
         return new FormBuilder<ConnectionDetails>()
            .Field(nameof(TfsUrl))
            .Field(nameof(TeamProject))
            .Field(nameof(PersonalAccessToken))
            .Field(nameof(RepositoryType))
            .Confirm(stringBuilder.ToString())
            .Build();
      }

      public void Clear()
      {
         TfsUrl = string.Empty;
         TeamProject = string.Empty;
         RepositoryType = null;
         PersonalAccessToken = string.Empty;
      }
   }
}