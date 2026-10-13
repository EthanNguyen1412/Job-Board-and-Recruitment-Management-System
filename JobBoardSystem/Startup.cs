using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JobBoardSystem.Startup))]
namespace JobBoardSystem
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
