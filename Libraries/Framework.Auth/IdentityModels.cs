using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Newtonsoft.Json;

using Framework.Common.Extensions;
using System.Web.Mvc;
using System.Data.Entity.ModelConfiguration;

namespace Framework.Auth
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public AuthDbContext() : base("AuthConnection")
        {
            //  Configuration.UseDatabaseNullSemantics = false;
            //Database.SetInitializer<IdentityDbContext>(null);// Remove default initializer
            Configuration.ProxyCreationEnabled = false;
            // Configuration.LazyLoadingEnabled = false;
        }

        static AuthDbContext()
        {
            // IdentityDbInit.InitialSetup(Create());
        }

        public static AuthDbContext Create()
        {
            return new AuthDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
              .ToTable("NcbUsers");

            modelBuilder.Entity<ApplicationRole>()
              .ToTable("NcbRoles");

            modelBuilder.Entity<IdentityUserRole>()
                .HasKey(r => new { UserId = r.UserId, RoleId = r.RoleId })
                 .ToTable("NcbUserRoles");

            modelBuilder.Entity<IdentityUserLogin>()
              .HasKey(u => new { u.UserId, u.ProviderKey, u.LoginProvider })
              .ToTable("NcbUserLogins");

            modelBuilder.Entity<IdentityUserClaim>()
              .HasKey(u => u.Id)
              .ToTable("NcbUserClaims");

            modelBuilder.Entity<ApplicationRole>().Property(r => r.Name)
              .IsRequired();
        }
    }

    internal class IdentityDbInit
    {
        public static void InitialSetup(AuthDbContext context)
        {
            // 初始化配置将放在这儿
            var userMgr = new ApplicationUserManager(new UserStore<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>(context));
            var roleMgr = new ApplicationRoleManager(new RoleStore<ApplicationRole, string, IdentityUserRole>(context));
            var rigthts = RightManager.GetDefaultRights(true);

            if (!roleMgr.RoleExists(AuthSetting.AdminRoleName))
            {
                roleMgr.Create(new ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = AuthSetting.AdminRoleName,
                    Operator = AuthSetting.Administrator,
                    Remark = "超级管理员角色，系统默认创建，不充许改动"
                });
            }
            else
            {
                var role = roleMgr.FindByName(AuthSetting.AdminRoleName);
                role.Rights = JsonConvert.SerializeObject(rigthts);
                roleMgr.Update(role);
            }

            ApplicationUser user = userMgr.FindByName(AuthSetting.Administrator);
            if (user == null)
            {
                userMgr.Create(new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = AuthSetting.Administrator,
                    Email = "sa@example.com",
                    Operator = AuthSetting.Administrator,
                    Remark = "超级管理员，系统默认创建，不充许改动"
                }, AuthSetting.AdminPassowrd);
                user = userMgr.FindByName(AuthSetting.Administrator);
            }

            if (!userMgr.IsInRole(user.Id, AuthSetting.AdminRoleName))
            {
                userMgr.AddToRole(user.Id, AuthSetting.AdminRoleName);
            }

            context.SaveChanges();
        }
    }

    public enum UserStates
    {
        [Display(Name = "正常")]
        Normal,
        [Display(Name = "锁定")]
        Locked,
        [Display(Name = "删除")]
        Delete
    }

    public class ApplicationUser : IdentityUser<string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        [DisplayName("用户名称")]
        [Required(ErrorMessage = "请输入{0}")]
        [StringLength(50, ErrorMessage = "{0}必需在{1}个字符以内")]
        public override string UserName { get; set; }
        [DisplayName("电子邮件")]
        [EmailAddress(ErrorMessage = "{0}格式有误")]
        [StringLength(50, ErrorMessage = "{0}必需在{1}个字符以内")]
        public override string Email { get; set; }
        [DisplayName("性别")]
        public byte Gender { get; set; }
        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        [Required]
        [StringLength(50, ErrorMessage = "{0}必需在{1}个字符以内")]
        [DisplayName("操作员")]
        public string Operator { get; set; }
        [DisplayName("状态")]
        public UserStates UserState { get; set; }
        [DisplayName("备注")]
        [StringLength(500, ErrorMessage = "{0}必需在{1}个字符以内")]
        public string Remark { get; set; }

        public string CreateTimeDisplay
        {
            get
            {
                return CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public string UserStateDisplay
        {
            get
            {
                return UserState.GetDisplayName();
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, string> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }
    public class ApplicationRole : IdentityRole<string, IdentityUserRole>
    {
        public ApplicationRole() : base() { }

        [DisplayName("权限")]
        public string Rights { get; set; }
        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        [DisplayName("操作员")]
        [StringLength(50, ErrorMessage = "{0}必需在{1}个字符以内")]
        public string Operator { get; set; }
        [DisplayName("备注")]
        [StringLength(500, ErrorMessage = "{0}必需在{1}个字符以内")]
        public string Remark { get; set; }

        [NotMapped]
        public string CreateDateDisplay
        {
            get
            {
                return CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [NotMapped]
        public List<ParentRightItem> RightList
        {
            get
            {
                if (string.IsNullOrEmpty(Rights))
                {
                    return RightManager.GetDefaultRights();
                }
                return JsonConvert.DeserializeObject<List<ParentRightItem>>(Rights);
            }
        }
    }

    //public class ApplicationUserRole:IdentityUserRole
    //{
    //    [ForeignKey("UserId")]
    //    public virtual IEnumerable<ApplicationUser> Users { get; set; }
    //    [ForeignKey("RoleId")]
    //    public virtual IEnumerable<ApplicationRole> Roles { get; set; }
    //}
}