using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SongOfTheDay.Startup))]
namespace SongOfTheDay
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
