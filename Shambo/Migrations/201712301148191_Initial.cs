namespace Shambo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subscriptions", "BuildDefinitionName", c => c.String());
            AddColumn("dbo.Subscriptions", "BuildState", c => c.String());
            DropColumn("dbo.Subscriptions", "BuildDefinitionNames");
            DropColumn("dbo.Subscriptions", "BuildStates");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Subscriptions", "BuildStates", c => c.String());
            AddColumn("dbo.Subscriptions", "BuildDefinitionNames", c => c.String());
            DropColumn("dbo.Subscriptions", "BuildState");
            DropColumn("dbo.Subscriptions", "BuildDefinitionName");
        }
    }
}
