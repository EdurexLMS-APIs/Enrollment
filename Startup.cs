using AspNetCoreHero.ToastNotification;
using CPEA.Data;
using CPEA.Models;
using CPEA.Utilities;
using CPEA.Utilities.Interface;
using CPEA.Utilities.Services;
//using DinkToPdf;
//using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPEA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
       
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            //services.AddMvc().AddXmlSerializerFormatters();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(5);
            });
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddScoped<IAddressServices, AddressServices>();
            services.AddScoped<IProjectServices, ProjectServices>();
            services.AddScoped<IRespayBulkUpload, RespayBulkUploadServices>();
            services.AddScoped<IMessagingService, MessagingService>();
            services.AddScoped<IAdminServices, AdminServices>();
            services.AddScoped<IInterSwitch, InterSwitchServcie>();
            services.AddScoped<IImageUpload, ImageUploadServices>();
            services.AddScoped<IEmail, EmailService>();


            services.AddIdentity<Users, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddNotyf(config =>
            {
               // config.DurationInSeconds = 10;
                config.IsDismissable = true;
                config.Position = NotyfPosition.TopRight;
            });

            var apiSettingsSection = Configuration.GetSection("APISettings");
            services.Configure<APISettings>(apiSettingsSection);
            var apiSettings = apiSettingsSection.Get<APISettings>();
            var monnifyKey = Encoding.ASCII.GetBytes(apiSettings.MonnifySecret);
            var monocoKey = Encoding.ASCII.GetBytes(apiSettings.MonocoSecretKey);


            var termiiSettingsSection = Configuration.GetSection("Termii");
            services.Configure<TermiiSettings>(termiiSettingsSection);
            var termiiSettings = termiiSettingsSection.Get<TermiiSettings>();


            var SendgridSettingsSection = Configuration.GetSection("Sendgrid");
            services.Configure<SendGridSettings>(SendgridSettingsSection);
            var SendgridSettings = SendgridSettingsSection.Get<SendGridSettings>();

            var SmtpSettingsSection = Configuration.GetSection("Smtp");
            services.Configure<SMTPSetting>(SmtpSettingsSection);
            var SmtpSettings = SmtpSettingsSection.Get<SMTPSetting>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || (env.IsProduction()))
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Enrollment}/{action=Register}/{id?}");
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                     name: "Admin",
                     pattern: "{controller=Admin}/{action=Login}/{id?}");
                endpoints.MapRazorPages();

            });
        }
    }
}
