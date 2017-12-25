using System;
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
         /*Validation if URL exists (ping) and PAT is correct?*/
         return new FormBuilder<ConnectionDetails>()
            .AddRemainingFields(new[] { nameof(Id) })
            .Build();
      }
   }
}