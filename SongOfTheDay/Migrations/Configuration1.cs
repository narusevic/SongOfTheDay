using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace SongOfTheDay.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Models.DbConnection>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
}