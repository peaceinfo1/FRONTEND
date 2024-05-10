using System;
using System.IO;
using BAL.Listings;
using DAL.LISTING;
using DAL.SHARED;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.SqlServer;
using BAL.Messaging.Contracts;
using DAL.AUDIT;
using BAL.Audit;
using DAL.CATEGORIES;
using BAL.Addresses;
using DAL.BILLING;
using BAL.Billing;
using BAL.Category;
using HUBS.Admin;
using HUBS.Listing;
using BAL.Dashboard.Listing;
using BAL.Dashboard.History;
using DAL.BANNER;
using BAL.Claims;
using BAL.Identity;
using BAL.Search;
using HUBS.Notifications;
using BAL.Keyword;
using BAL.Claims.Listing;
using Microsoft.Net.Http.Headers;
using DAL.USER;
using DAL.Models;
using BAL.Services.Contracts;
using BAL.Services;
using DAL.Repositories.Contracts;
using DAL.Repositories;
using BAL.Messaging;
using BAL;

namespace FRONTEND
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddControllersWithViews();
#if DEBUG
            if (HostEnvironment.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation(options =>
                {
                    var components = Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, "..\\..\\", "COM"));
                    options.FileProviders.Add(new PhysicalFileProvider(components));
                });

                builder.AddRazorRuntimeCompilation(options =>
                {
                    var classified = Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, "..\\..\\", "CLASSIFIED"));
                    options.FileProviders.Add(new PhysicalFileProvider(classified));
                });

                builder.AddRazorRuntimeCompilation(options =>
                {
                    var identity = Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, "..\\..\\", "IDENTITY"));
                    options.FileProviders.Add(new PhysicalFileProvider(identity));
                });

                builder.AddRazorRuntimeCompilation(options =>
                {
                    var admin = Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, "..\\..\\", "ADMIN"));
                    options.FileProviders.Add(new PhysicalFileProvider(admin));
                });

                builder.AddRazorRuntimeCompilation(options =>
                {
                    var staff = Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, "..\\..\\", "STAFF"));
                    options.FileProviders.Add(new PhysicalFileProvider(staff));
                });
            }
#endif
            // Shafi: UserDbContext
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MimUser")));
            // End:

            // Shafi: SharedDbContext
            services.AddDbContext<SharedDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MimShared")));
            // End:

            // Shafi: ListingDbContext
            services.AddDbContext<ListingDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("MimListing")));
            // End:

            // Begin: ListingDbContext
            services.AddDbContext<CategoriesDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MimCategories")));
            // End:

            // Shafi: AuditDbContext
            services.AddDbContext<AuditDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("AuditTrail")));
            // End:

            // Begin: BillingDbContext
            services.AddDbContext<BillingDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MimBilling")));
            // End:

            // Begin: BannerDbContext
            services.AddDbContext<BannerDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MimBanner")));
            // End:

            // Shafi: 
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                // Shafi: Add role to get all roles in manage role controller
                .AddRoles<IdentityRole>()
                // End:
                .AddEntityFrameworkStores<UserDbContext>();

            // Shafi: Cookie based authentication and login
            // ConfigureApplicationCookie must be called after calling AddIdentity or AddDefaultIdentity.
            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-3.1
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "MyInteriorMart";
                options.Cookie.HttpOnly = false;
                options.ExpireTimeSpan = TimeSpan.FromDays(10);
                options.LoginPath = "/Identity/Account/Login";
                // ReturnUrlParameter requires 
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });
            // End:

            // Shafi: Added this becuse customer login and registration sessions was not working
            // Remove if it conflict with any module
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            // End:

            // Shafi: Customize Password And Other Policies
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;


                // Lockout settings.
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = false;

            });
            // End:

            // Shafi:
            services.AddTransient<IHistoryAudit, HistoryAudit>();
            // End:

            // Shafi: Catalog
            services.AddTransient<IListingManager, ListingManager>();
            // End:

            // Shafi: Respository
            services.AddTransient<IAddresses, Addresses>();
            services.AddTransient<IBilling, Billing>();
            services.AddTransient<ICategory, Category>();
            services.AddTransient<ICategory, Category>();
            services.AddTransient<IAddresses, Addresses>();
            services.AddTransient<IListingManager, ListingManager>();
            services.AddTransient<IBilling, Billing>();
            services.AddTransient<IMessageMailService, MessageMailService>();
            services.AddTransient<IHistoryAudit, HistoryAudit>();
            services.AddTransient<IDashboardListing, DashboardListing>();
            services.AddTransient<IDashboardUserHistory, DashboardUserHistory>();
            services.AddTransient<IClaimsAdmin, ClaimsAdmin>();
            services.AddTransient<IUserRoleClaim, UserRoleClaim>();
            services.AddTransient<ISearch, Search>();
            services.AddTransient<IUsersOnlineRepository, UsersOnlineRepository>();
            services.AddTransient<IKeywords, Keywords>();
            services.AddTransient<IListingStaff, ListingStaff>();
            services.AddTransient<IClaimListing, ClaimListing>();
            services.AddTransient<ISuspendedUserService, SuspendedUserService>();
            services.AddTransient<ISuspendedUserRepository, SuspendedUserRepository>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IUserProfileRepository, UserProfileRepository>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IListingService, ListingService>();
            services.AddTransient<IListingRepository, ListingRepository>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ISharedService, SharedService>();
            services.AddTransient<ISharedRepository, SharedRepository>();
            services.AddTransient<IAuditService, AuditService>();
            services.AddTransient<IAuditRepository, AuditRepository>();
            services.AddTransient<IMessageMailService, MessageMailService>();
            services.AddTransient<HelperFunctions>();
            // End:

            // Shafi: Add Claim based authorization
            services.AddAuthorization(options =>
            {
                // Shafi: Admin Dashboard
                options.AddPolicy("Admin-Dashboard-Realtime-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Realtime.View"));
                options.AddPolicy("Admin-Dashboard-Listings-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Listings.View"));
                options.AddPolicy("Admin-Dashboard-Users-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Users.View"));
                options.AddPolicy("Admin-Dashboard-Analytics-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Analytics.View"));
                options.AddPolicy("Admin-Dashboard-Notifications-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Notifications.View"));
                options.AddPolicy("Admin-Dashboard-Search-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Search.View"));
                options.AddPolicy("Admin-Dashboard-Enquiries-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Enquiries.View"));
                options.AddPolicy("Admin-Dashboard-Marketing-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Marketing.View"));
                options.AddPolicy("Admin-Dashboard-Billing-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Billing.View"));
                options.AddPolicy("Admin-Dashboard-Staff-View", policy => policy.RequireClaim("Admin.Dashboard", "Admin.Dashboard.Staff.View"));
                // End:

                // Shafi: Admin Users & Roles
                options.AddPolicy("Admin-Users-ViewAll", policy => policy.RequireClaim("Admin.Users", "Admin.Users.ViewAll"));
                options.AddPolicy("Admin-Users-ViewProfile", policy => policy.RequireClaim("Admin.Users", "Admin.Users.ViewProfile"));
                options.AddPolicy("Admin-Users-BlockUser", policy => policy.RequireClaim("Admin.Users", "Admin.Users.BlockUser"));
                options.AddPolicy("Admin-Users-UnblockUser", policy => policy.RequireClaim("Admin.Users", "Admin.Users.UnblockUser"));
                options.AddPolicy("Admin-Users-ViewAllBlockedUser", policy => policy.RequireClaim("Admin.Users", "Admin.Users.ViewAllBlockedUser"));
                options.AddPolicy("Admin-Users-ViewAllUnblockedUser", policy => policy.RequireClaim("Admin.Users", "Admin.Users.ViewAllUnblockedUser"));
                // End:

                // Shafi: Roles
                options.AddPolicy("Admin-Roles-ViewAll", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.ViewAll"));
                options.AddPolicy("Admin-Roles-Read", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.Read"));
                options.AddPolicy("Admin-Roles-Edit", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.Edit"));
                options.AddPolicy("Admin-Roles-Delete", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.Delete"));
                options.AddPolicy("Admin-Roles-Create", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.Create"));
                options.AddPolicy("Admin-Roles-AssignRoleToUser", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.AssignRoleToUser"));
                options.AddPolicy("Admin-Roles-RemoveUserFromRole", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.RemoveUserFromRole"));
                options.AddPolicy("Admin-Roles-AssignClaimToRole", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.AssignClaimToRole"));
                options.AddPolicy("Admin-Roles-ListUsersInRole", policy => policy.RequireClaim("Admin.Users", "Admin.Roles.ListUsersInRole"));
                // End:

                // Shafi: Admin Role Ctegories
                options.AddPolicy("Admin-RoleCategories-ViewAll", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCategories.ViewAll"));
                options.AddPolicy("Admin-RoleCategories-Read", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCategories.Read"));
                options.AddPolicy("Admin-RoleCategories-Create", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCategories.Create"));
                options.AddPolicy("Admin-RoleCategories-Edit", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCategories.Edit"));
                options.AddPolicy("Admin-RoleCategories-Delete", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCategories.Delete"));
                // End:

                // Shafi: Admin Role Categories & Roles
                options.AddPolicy("Admin-RoleCatNRole-ViewAll", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCatNRole.ViewAll"));

                options.AddPolicy("Admin-RoleCatNRole-Read", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCatNRole.Read"));

                options.AddPolicy("Admin-RoleCatNRole-Create", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCatNRole.Create"));

                options.AddPolicy("Admin-RoleCatNRole-Edit", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCatNRole.Edit"));

                options.AddPolicy("Admin-RoleCatNRole-Delete", policy => policy.RequireClaim("Admin.Users", "Admin.RoleCatNRole.Delete"));
                // End:

                // Shafi: Admin Listings
                options.AddPolicy("Admin-Listing-ViewAll", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.ViewAll"));
                options.AddPolicy("Admin-Listing-Read", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.Read"));
                options.AddPolicy("Admin-Listing-Create", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.Create"));
                options.AddPolicy("Admin-Listing-Edit", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.Edit"));
                options.AddPolicy("Admin-Listing-Delete", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.Delete"));
                options.AddPolicy("Admin-Listing-Approve", policy => policy.RequireClaim("Admin.Listings", "Admin.Listing.Approve"));
                options.AddPolicy("Admin-Review-ViewAll", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.ViewAll"));
                options.AddPolicy("Admin-Review-Read", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.Read"));
                options.AddPolicy("Admin-Review-Create", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.Create"));
                options.AddPolicy("Admin-Review-Edit", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.Edit"));
                options.AddPolicy("Admin-Review-Delete", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.Delete"));
                options.AddPolicy("Admin-Review-Approve", policy => policy.RequireClaim("Admin.Listings", "Admin.Review.Approve"));
                options.AddPolicy("Admin-Like-ViewAll", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.ViewAll"));
                options.AddPolicy("Admin-Like-Read", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.Read"));
                options.AddPolicy("Admin-Like-Create", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.Create"));
                options.AddPolicy("Admin-Like-Edit", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.Edit"));
                options.AddPolicy("Admin-Like-Delete", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.Delete"));
                options.AddPolicy("Admin-Like-Approve", policy => policy.RequireClaim("Admin.Listings", "Admin.Like.Approve"));
                options.AddPolicy("Admin-Subscribe-ViewAll", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.ViewAll"));
                options.AddPolicy("Admin-Subscribe-Read", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.Read"));
                options.AddPolicy("Admin-Subscribe-Create", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.Create"));
                options.AddPolicy("Admin-Subscribe-Edit", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.Edit"));
                options.AddPolicy("Admin-Subscribe-Delete", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.Delete"));
                options.AddPolicy("Admin-Subscribe-Approve", policy => policy.RequireClaim("Admin.Listings", "Admin.Subscribe.Approve"));
                options.AddPolicy("Admin-Bookmark-ViewAll", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.ViewAll"));
                options.AddPolicy("Admin-Bookmark-Read", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.Read"));
                options.AddPolicy("Admin-Bookmark-Create", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.Create"));
                options.AddPolicy("Admin-Bookmark-Edit", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.Edit"));
                options.AddPolicy("Admin-Bookmark-Delete", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.Delete"));
                options.AddPolicy("Admin-Bookmark-Approve", policy => policy.RequireClaim("Admin.Listings", "Admin.Bookmark.Approve"));
                // End:

                // Shafi: Admin Localities
                options.AddPolicy("Admin-Country-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.Country.ViewAll"));
                options.AddPolicy("Admin-Country-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.Country.Read"));
                options.AddPolicy("Admin-Country-Create", policy => policy.RequireClaim("Admin.Locality", "Admin.Country.Create"));
                options.AddPolicy("Admin-Country-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.Country.Edit"));
                options.AddPolicy("Admin-Country-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.Country.Delete"));
                options.AddPolicy("Admin-State-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.State.ViewAll"));
                options.AddPolicy("Admin-State-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.State.Read"));
                options.AddPolicy("Admin-State-Create", policy => policy.RequireClaim("Admin.Locality", "Admin.State.Create"));
                options.AddPolicy("Admin-State-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.State.Edit"));
                options.AddPolicy("Admin-State-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.State.Delete"));
                options.AddPolicy("Admin-City-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.City.ViewAll"));
                options.AddPolicy("Admin-City-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.City.Read"));
                options.AddPolicy("Admin-City-Create", policy => policy.RequireClaim("Admin.Locality", "Admin.City.Create"));
                options.AddPolicy("Admin-City-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.City.Edit"));
                options.AddPolicy("Admin-City-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.City.Delete"));
                options.AddPolicy("Admin-Assembly-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.Assembly.ViewAll"));
                options.AddPolicy("Admin-Assembly-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.Assembly.Read"));
                options.AddPolicy("Admin-Assembly-Create", policy => policy.RequireClaim("Admin.Locality", "Admin.Assembly.Create"));
                options.AddPolicy("Admin-Assembly-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.Assembly.Edit"));
                options.AddPolicy("Admin-Assembly-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.Assembly.Delete"));
                options.AddPolicy("Admin-Pincode-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.Pincode.ViewAll"));
                options.AddPolicy("Admin-Pincode-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.Pincode.Read"));
                options.AddPolicy("Admin-Pincode-Create", policy => policy.RequireClaim("Admin.Locality", "Admin.Pincode.Create"));
                options.AddPolicy("Admin-Pincode-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.Pincode.Edit"));
                options.AddPolicy("Admin-Pincode-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.Pincode.Delete"));
                options.AddPolicy("Admin-Area-ViewAll", policy => policy.RequireClaim("Admin.Locality", "Admin.Area.ViewAll"));
                options.AddPolicy("Admin-Area-Read", policy => policy.RequireClaim("Admin.Locality", "Admin.Area.Read"));
                options.AddPolicy("Admin-Area-Create", policy => policy.RequireClaim("Admin.Locality", "Admin-Area-Create"));
                options.AddPolicy("Admin-Area-Edit", policy => policy.RequireClaim("Admin.Locality", "Admin.Area.Edit"));
                options.AddPolicy("Admin-Area-Delete", policy => policy.RequireClaim("Admin.Locality", "Admin.Area.Delete"));
                // End:

                // Shafi: Admin Categories
                options.AddPolicy("Admin-FirstCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.FirstCategory.ViewAll"));
                options.AddPolicy("Admin-FirstCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.FirstCategory.Read"));
                options.AddPolicy("Admin-FirstCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.FirstCategory.Create"));
                options.AddPolicy("Admin-FirstCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.FirstCategory.Edit"));
                options.AddPolicy("Admin-FirstCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.FirstCategory.Delete"));
                options.AddPolicy("Admin-SecondCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.SecondCategory.ViewAll"));
                options.AddPolicy("Admin-SecondCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.SecondCategory.Read"));
                options.AddPolicy("Admin-SecondCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.SecondCategory.Create"));
                options.AddPolicy("Admin-SecondCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.SecondCategory.Edit"));
                options.AddPolicy("Admin-SecondCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.SecondCategory.Delete"));
                options.AddPolicy("Admin-ThirdCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.ThirdCategory.ViewAll"));
                options.AddPolicy("Admin-ThirdCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.ThirdCategory.Read"));
                options.AddPolicy("Admin-ThirdCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.ThirdCategory.Create"));
                options.AddPolicy("Admin-ThirdCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.ThirdCategory.Edit"));
                options.AddPolicy("Admin-ThirdCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.ThirdCategory.Delete"));
                options.AddPolicy("Admin-FourthCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.FourthCategory.ViewAll"));
                options.AddPolicy("Admin-FourthCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.FourthCategory.Read"));
                options.AddPolicy("Admin-FourthCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.FourthCategory.Create"));
                options.AddPolicy("Admin-FourthCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.FourthCategory.Edit"));
                options.AddPolicy("Admin-FourthCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.FourthCategory.Delete"));
                options.AddPolicy("Admin-FifthCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.FifthCategory.ViewAll"));
                options.AddPolicy("Admin-FifthCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.FifthCategory.Read"));
                options.AddPolicy("Admin-FifthCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.FifthCategory.Create"));
                options.AddPolicy("Admin-FifthCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.FifthCategory.Edit"));
                options.AddPolicy("Admin-FifthCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.FifthCategory.Delete"));
                options.AddPolicy("Admin-SixthCategory-ViewAll", policy => policy.RequireClaim("Admin.Categories", "Admin.SixthCategory.ViewAll"));
                options.AddPolicy("Admin-SixthCategory-Read", policy => policy.RequireClaim("Admin.Categories", "Admin.SixthCategory.Read"));
                options.AddPolicy("Admin-SixthCategory-Create", policy => policy.RequireClaim("Admin.Categories", "Admin.SixthCategory.Create"));
                options.AddPolicy("Admin-SixthCategory-Edit", policy => policy.RequireClaim("Admin.Categories", "Admin.SixthCategory.Edit"));
                options.AddPolicy("Admin-SixthCategory-Delete", policy => policy.RequireClaim("Admin.Categories", "Admin.SixthCategory.Delete"));
                // End:

                // Shafi: Admin Miscellaneous
                options.AddPolicy("Admin-Designation-ViewAll", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Designation.ViewAll"));
                options.AddPolicy("Admin-Designation-Read", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Designation.Read"));
                options.AddPolicy("Admin-Designation-Create", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Designation.Create"));
                options.AddPolicy("Admin-Designation-Edit", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Designation.Edit"));
                options.AddPolicy("Admin-Designation-Delete", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Designation.Delete"));
                options.AddPolicy("Admin-NatureofBusiness-ViewAll", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.NatureofBusiness.ViewAll"));
                options.AddPolicy("Admin-NatureofBusiness-Read", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.NatureofBusiness.Read"));
                options.AddPolicy("Admin-NatureofBusiness-Create", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.NatureofBusiness.Create"));
                options.AddPolicy("Admin-NatureofBusiness-Edit", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.NatureofBusiness.Edit"));
                options.AddPolicy("Admin-NatureofBusiness-Delete", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.NatureofBusiness.Delete"));
                options.AddPolicy("Admin-Turnover-ViewAll", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Turnover.ViewAll"));
                options.AddPolicy("Admin-Turnover-Read", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Turnover.Read"));
                options.AddPolicy("Admin-Turnover-Create", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Turnover.Create"));
                options.AddPolicy("Admin-Turnover-Edit", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Turnover.Edit"));
                options.AddPolicy("Admin-Turnover-Delete", policy => policy.RequireClaim("Admin.Miscellaneous", "Admin.Turnover.Delete"));
                // End:

                // Shafi: Admin Keywords
                options.AddPolicy("Admin-FirstCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.FirstCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-FirstCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.FirstCategoryKeyword.Read"));
                options.AddPolicy("Admin-FirstCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.FirstCategoryKeyword.Create"));
                options.AddPolicy("Admin-FirstCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.FirstCategoryKeyword.Edit"));
                options.AddPolicy("Admin-FirstCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.FirstCategoryKeyword.Delete"));
                options.AddPolicy("Admin-SecondCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.SecondCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-SecondCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.SecondCategoryKeyword.Read"));
                options.AddPolicy("Admin-SecondCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.SecondCategoryKeyword.Create"));
                options.AddPolicy("Admin-SecondCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.SecondCategoryKeyword.Edit"));
                options.AddPolicy("Admin-SecondCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.SecondCategoryKeyword.Delete"));
                options.AddPolicy("Admin-ThirdCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.ThirdCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-ThirdCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.ThirdCategoryKeyword.Read"));
                options.AddPolicy("Admin-ThirdCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.ThirdCategoryKeyword.Create"));
                options.AddPolicy("Admin-ThirdCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.ThirdCategoryKeyword.Edit"));
                options.AddPolicy("Admin-ThirdCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.ThirdCategoryKeyword.Delete"));
                options.AddPolicy("Admin-FourthCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.FourthCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-FourthCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.FourthCategoryKeyword.Read"));
                options.AddPolicy("Admin-FourthCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.FourthCategoryKeyword.Create"));
                options.AddPolicy("Admin-FourthCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.FourthCategoryKeyword.Edit"));
                options.AddPolicy("Admin-FourthCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.FourthCategoryKeyword.Delete"));
                options.AddPolicy("Admin-FifthCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.FifthCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-FifthCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.FifthCategoryKeyword.Read"));
                options.AddPolicy("Admin-FifthCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.FifthCategoryKeyword.Create"));
                options.AddPolicy("Admin-FifthCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.FifthCategoryKeyword.Edit"));
                options.AddPolicy("Admin-FifthCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.FifthCategoryKeyword.Delete"));
                options.AddPolicy("Admin-SixthCategoryKeyword-ViewAll", policy => policy.RequireClaim("Admin.Keywords", "Admin.SixthCategoryKeyword.ViewAll"));
                options.AddPolicy("Admin-SixthCategoryKeyword-Read", policy => policy.RequireClaim("Admin.Keywords", "Admin.SixthCategoryKeyword.Read"));
                options.AddPolicy("Admin-SixthCategoryKeyword-Create", policy => policy.RequireClaim("Admin.Keywords", "Admin.SixthCategoryKeyword.Create"));
                options.AddPolicy("Admin-SixthCategoryKeyword-Edit", policy => policy.RequireClaim("Admin.Keywords", "Admin.SixthCategoryKeyword.Edit"));
                options.AddPolicy("Admin-SixthCategoryKeyword-Delete", policy => policy.RequireClaim("Admin.Keywords", "Admin.SixthCategoryKeyword.Delete"));
                // End:

                // Shafi: Admin Pages
                options.AddPolicy("Admin-Page-ViewAll", policy => policy.RequireClaim("Admin.Pages", "Admin.Page.ViewAll"));
                options.AddPolicy("Admin-Page-Read", policy => policy.RequireClaim("Admin.Pages", "Admin.Page.Read"));
                options.AddPolicy("Admin-Page-Create", policy => policy.RequireClaim("Admin.Pages", "Admin.Page.Create"));
                options.AddPolicy("Admin-Page-Edit", policy => policy.RequireClaim("Admin.Pages", "Admin.Page.Edit"));
                options.AddPolicy("Admin-Page-Delete", policy => policy.RequireClaim("Admin.Pages", "Admin.Page.Delete"));
                // End:

                // Shafi: Admin Notifications
                options.AddPolicy("Admin-Notification-ViewAll", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.ViewAll"));
                options.AddPolicy("Admin-Notification-Read", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.Read"));
                options.AddPolicy("Admin-Notification-Create", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.Create"));
                options.AddPolicy("Admin-Notification-Edit", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.Edit"));
                options.AddPolicy("Admin-Notification-Delete", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.Delete"));
                options.AddPolicy("Admin-Notification-ReceiveSMS", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.ReceiveSMS"));
                options.AddPolicy("Admin-Notification-ReceiveEmail", policy => policy.RequireClaim("Admin.Notifications", "Admin.Notification.ReceiveEmail"));
                // End:

                // Shafi: Admin Slideshow
                options.AddPolicy("Admin-Slideshow-ViewAll", policy => policy.RequireClaim("Admin.Slideshow", "Admin.Slideshow.ViewAll"));
                options.AddPolicy("Admin-Slideshow-Read", policy => policy.RequireClaim("Admin.Slideshow", "Admin.Slideshow.Read"));
                options.AddPolicy("Admin-Slideshow-Create", policy => policy.RequireClaim("Admin.Slideshow", "Admin.Slideshow.Create"));
                options.AddPolicy("Admin-Slideshow-Edit", policy => policy.RequireClaim("Admin.Slideshow", "Admin.Slideshow.Edit"));
                options.AddPolicy("Admin-Slideshow-Delete", policy => policy.RequireClaim("Admin.Slideshow", "Admin.Slideshow.Delete"));
                // End:

                // Shafi: Admin History And Cache
                options.AddPolicy("Admin-UserHistory-ViewAll", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.UserHistory.ViewAll"));
                options.AddPolicy("Admin-UserHistory-Read", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.UserHistory.Read"));
                options.AddPolicy("Admin-UserHistory-Create", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.UserHistory.Create"));
                options.AddPolicy("Admin-UserHistory-Edit", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.UserHistory.Edit"));
                options.AddPolicy("Admin-UserHistory-Delete", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.UserHistory.Delete"));
                options.AddPolicy("Admin-ListingHistory-ViewAll", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.ListingHistory.ViewAll"));
                options.AddPolicy("Admin-ListingHistory-Read", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.ListingHistory.Read"));
                options.AddPolicy("Admin-ListingHistory-Create", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.ListingHistory.Create"));
                options.AddPolicy("Admin-ListingHistory-Edit", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.ListingHistory.Edit"));
                options.AddPolicy("Admin-ListingHistory-Delete", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.ListingHistory.Delete"));
                options.AddPolicy("Admin-Suggestion-ViewAll", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.ViewAll"));
                options.AddPolicy("Admin-Suggestion-Read", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.Read"));
                options.AddPolicy("Admin-Suggestion-Create", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.Create"));
                options.AddPolicy("Admin-Suggestion-Edit", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.Edit"));
                options.AddPolicy("Admin-Suggestion-Delete", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.Delete"));
                options.AddPolicy("Admin-Suggestion-ViewCache", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.ViewCache"));
                options.AddPolicy("Admin-Suggestion-ClearCache", policy => policy.RequireClaim("Admin.HistoryAndCache", "Admin.Suggestion.ClearCache"));
                // End:
            });
            // End:

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
            });

            // Shafi: Added to enable update razor view after page refreshing
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            // End:

            // Shafi: Hangfire
            services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(Configuration.GetConnectionString("AuditTrail"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue = true,
                DisableGlobalLocks = true
            }));
            services.AddHangfireServer();
            // End:

            // Shafi: SignalR
            services.AddSignalR();
            // End:

            // Shafi: Response Caching
            services.AddMemoryCache();
            // End:

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseHangfireDashboard("/hangfire");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });

            app.UseRouting();

            app.UseSession();

            // Shafi: Hangfire
            app.UseHangfireDashboard();
            // End: Hangfire

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Shafi: SignalR Hub References from HUBS project
                endpoints.MapHub<AllConnectedUsersHub>("/allConnectedUsersHub");
                endpoints.MapHub<UsersOnlineHomeHub>("/usersOnlineHomeHub");
                endpoints.MapHub<UsersOnlineCategoriesHub>("/usersOnlineCategoriesHub");
                endpoints.MapHub<UsersOnlineListingHub>("/usersOnlineListingHub");
                endpoints.MapHub<UsersOnlineDashboardHub>("/usersOnlineDashboardHub");
                endpoints.MapHub<AllConnectedUsersMapHub>("/allConnectedUsersMapHub");
                endpoints.MapHub<AllConnectedUsersMapHub>("/allConnectedUsersMapHub");
                endpoints.MapHub<AllConnectedUsersMapHub>("/allConnectedUsersMapHub");
                endpoints.MapHub<UsersOnlineOnListingHub>("/usersOnlineOnListingHub");
                // End:

                // Shafi: SignalR Hub for User Dashboard
                endpoints.MapHub<NotificationUserDashboardHub>("/notificationUserDashboardHub");
                // End:

                // Shafi: Reference To Component Project
                endpoints.MapControllerRoute(
                       name: "Component",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
                       new string[] { "COM.Controllers" }
                       );
                // End:

                // Shafi: Reference To Admin Project
                endpoints.MapControllerRoute(
                       name: "Admin",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
                       new string[] { "ADMIN.Controllers" }
                       );
                // End:

                // Shafi: Reference To Identity
                endpoints.MapControllerRoute(
                       name: "IDENTITY",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
                       new string[] { "IDENTITY.Controllers" }
                       );
                // End:

                // Shafi: Reference To Identity
                endpoints.MapControllerRoute(
                       name: "DASHBOARD",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
                       new string[] { "DASHBOARD.Controllers" }
                       );
                // End:

                // Shafi: Configure route for Areas
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                // End:

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Shafi: Identity/Account/Login current project
                // Note: Add this code in every startup file which reference to IDENTITY projct
                // If endpoints.MapRazorPages() not placed in startup.cs then identity will not work
                endpoints.MapRazorPages();
                // End:
            });
        }
    }
}