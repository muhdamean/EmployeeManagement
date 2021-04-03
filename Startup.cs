using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_config.GetConnectionString("EmployeeDbCon")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options=> 
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                //account lockout settings
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })//IdentityUser is the default class for Identity, ApplicationUser is extended
             .AddEntityFrameworkStores<AppDbContext>()
             .AddDefaultTokenProviders()
             .AddTokenProvider<CustomEmailConfrimationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");
            //for configuring the default password rules. can be done in IdentityRole above with lambda exp.
            services.Configure<PasswordOptions>(options =>
            {
                options.RequiredLength = 8;
                //options.RequireNonAlphanumeric = false;
            });
            //modifying token lifespan from default(1 day)
            services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(5));
            //configuring custom email confirmation token lifespan
            services.Configure<CustomEmailConfrimationTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromDays(3));

            services.AddMvc(options => {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //service for login authorization
                options.Filters.Add(new AuthorizeFilter(policy)); // adding simple authorization to make sure all users are authorize
            }).AddXmlSerializerFormatters();
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "43838323334-gvofi8pva9pt7ou68g8tutq48c1siceb.apps.googleusercontent.com";
                options.ClientSecret = "JGMf9D69xcTRBdbU3r1NkXgF";
            }).AddFacebook(options =>
            {
                options.ClientId = "3003166386579339";
                options.AppSecret = "eaeab9be2215a3a481fec08255219fd3";
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            services.AddAuthorization(options =>
            {
                //Claims Policy
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role","true"));
                //.RequireClaim("Create Role"));//only Delete Role claim required, dot RequireClaim means both claim required.

                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequiement())); //custom auth with no personal role edit

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context=>context.User.IsInRole("Admin")&& 
                //                                                                        context.User.HasClaim(claim=>claim.Type=="Edit Role" && claim.Value=="true")||
                //                                                                        context.User.IsInRole("Super Admin"))); //both (Admin && Edit Role) || Super Admin
                options.AddPolicy("CreateRolePolicy", policy => policy.RequireClaim("Create Role","true"));

                //Role Policy
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin")); // dot RequireRole("Admin","User","Manager") for multiple roles in the policy
            });
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddTransient<IAuthorizationHandler, CanOnlyEditOtherAdminRolesAndClaimsHandler>();
            services.AddTransient<IAuthorizationHandler, SuperAdminHandler>();
            services.AddTransient<DataProtectionPurposeStrings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}"); //url does not change
                //app.UseStatusCodePagesWithRedirects("Error/{0}"); //url changes to error page url
            }
            //app.UseGlobalExceptionMiddleware();
            app.UseFileServer(); //static files
            //app.UseMvcWithDefaultRoute();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization(); //middleware that checks for user authorization

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //endpoints.MapGet("/", async context =>
                //{
                //    //Exception ex=new Exception();
                //    // await context.Response.WriteAsync(ex.StackTrace);
                //    await context.Response.WriteAsync("Hello World!");

                //});
            });  
        }
    }
}
