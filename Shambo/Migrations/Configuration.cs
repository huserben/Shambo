namespace Shambo.Migrations
{
   using System.Data.Entity.Migrations;

   internal sealed class Configuration : DbMigrationsConfiguration<Shambo.Models.BotConfigContext>
    {
      /*
       * Add-Migration Initial
       * Update-Database
       */
      public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Shambo.Models.BotConfigContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
