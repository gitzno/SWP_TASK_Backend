using Microsoft.AspNetCore.Builder;

namespace SWP_Login
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        }
        public void ConfigureServices(IServiceCollection services, string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            services.AddAuthentication()
            .AddGoogle(googleOptions =>
            { 
            IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
            googleOptions.ClientId = googleAuthNSection["ClientId"];
            googleOptions.ClientSecret = googleAuthNSection["ClientSecret"]; 
            googleOptions.CallbackPath = "/api/Login/google";
            });
        }
    }
}
