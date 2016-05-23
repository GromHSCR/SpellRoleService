using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SpellRoleService.Startup))]
namespace SpellRoleService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        this.ConfigureAuth(app);
			app.MapSignalR();
		}
    }
}
