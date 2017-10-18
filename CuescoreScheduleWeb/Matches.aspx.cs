using CuescoreSchedule;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CuescoreScheduleWeb
{
    public partial class _Matches : Page
    {
        private CuescoreParser parser;
        private HtmlDocument leagueDocument;
        public string LeagueID;
        public string TeamName;
        public List<Appointment> Appointments;

        protected void Page_Load(object sender, EventArgs e)
        {
            parser = new CuescoreParser();
            LeagueID = Request.Url.Segments[2];
            TeamName = Request.Url.Segments[3];
            leagueDocument = parser.GetLeagueDocument(LeagueID);
            Appointments = parser.GetTeamAppointments(leagueDocument, Uri.UnescapeDataString(TeamName));
        }

        public void DownloadICAL(Object sender, EventArgs e)
        {
            Response.AddHeader("Content-Type", "application/octet-stream");
            Response.AddHeader("Content-Transfer-Encoding", "Text");
            Response.AddHeader("Content-disposition", "attachment; filename=\"" + TeamName + ".ics\"");
            var content = ICALGenerator.GetICALEventsContent(Appointments);
            Response.Write(content);
            Response.End();
        }
    }
}