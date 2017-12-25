using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shambo.Model.Botdata
{
   [Table("BotConfiguration")]
   public class BotConfiguration
   {
      [Key]
      public int Id { get; set; }

      [ForeignKey(nameof(ConnectionDetails))]
      public int ConnectionDetailsId { get; set; }

      public ConnectionDetails ConnectionDetails { get; set; }

      public List<Subscription> Subscriptions { get; set; }
   }
}