namespace Shambo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subscriptions", "BuildStates", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subscriptions", "BuildStates");
        }
    }
}
