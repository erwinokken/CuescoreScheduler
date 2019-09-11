using CuescoreSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using League = CuescoreSchedule.League;

namespace CuescoreScheduleWeb
{
    public partial class _Default : Page
    {
        private CuescoreParser parser;

        protected void Page_Load(object sender, EventArgs e)
        {
            parser = new CuescoreParser();
        }

        protected List<League> GetLeagues()
        {
            return parser.GetLeagues();
        }
    }
}