namespace Framework.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NcbRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Rights = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        Operator = c.String(maxLength: 50),
                        Remark = c.String(maxLength: 500),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.NcbUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.NcbRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.NcbUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.NcbUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Email = c.String(maxLength: 256),
                        Gender = c.Byte(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        Operator = c.String(nullable: false, maxLength: 50),
                        UserState = c.Int(nullable: false),
                        Remark = c.String(maxLength: 500),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.NcbUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NcbUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.NcbUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.ProviderKey, t.LoginProvider })
                .ForeignKey("dbo.NcbUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NcbUserRoles", "UserId", "dbo.NcbUsers");
            DropForeignKey("dbo.NcbUserLogins", "UserId", "dbo.NcbUsers");
            DropForeignKey("dbo.NcbUserClaims", "UserId", "dbo.NcbUsers");
            DropForeignKey("dbo.NcbUserRoles", "RoleId", "dbo.NcbRoles");
            DropIndex("dbo.NcbUserLogins", new[] { "UserId" });
            DropIndex("dbo.NcbUserClaims", new[] { "UserId" });
            DropIndex("dbo.NcbUsers", "UserNameIndex");
            DropIndex("dbo.NcbUserRoles", new[] { "RoleId" });
            DropIndex("dbo.NcbUserRoles", new[] { "UserId" });
            DropIndex("dbo.NcbRoles", "RoleNameIndex");
            DropTable("dbo.NcbUserLogins");
            DropTable("dbo.NcbUserClaims");
            DropTable("dbo.NcbUsers");
            DropTable("dbo.NcbUserRoles");
            DropTable("dbo.NcbRoles");
        }
    }
}
