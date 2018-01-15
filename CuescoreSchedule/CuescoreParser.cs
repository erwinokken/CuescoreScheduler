using System;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Caching;

/*
 *  Bugs:
 *      - 
 *  ToDo:
 *      - 
 *  Changelog:
 *      - Feature gemaakt waarbij je het team kan kiezen
 *      - Bug opgelost waarbij gespeelde wedstrijden niet meegenomen werden
 *      - Bug opgelost waar hij te weinig matches pakte (waar een team thuis speelde)
 *      - Bug opgelost waarbij jaar niet opgehoogt werd. Jaar is niet bekend, stel vorige is December, dus Januari is niet dit jaar, maar volgend jaar.
 *      - 15-1-2018: Bug opgelost -- AANPASSING VAN CUESCORE --
 */
namespace CuescoreSchedule
{
    public class CuescoreParser
    {
        // Example: https://cuescore.com/tournament/2017%252F2018+Pool+Noord+Eerste+Klasse/1548571
        private const string LEAGUE_URL = "https://cuescore.com/tournament/league_title/{0}";
        private const string LEAGUES_URL = "https://cuescore.com/live/league/";
        private DateTime lastDateTime;
        private int _currentYear;
        
        public CuescoreParser()
        {
            _currentYear = DateTime.Now.Year;
        }

        private HtmlDocument GetDocumentCache(string url)
        {
            if (HttpRuntime.Cache[url] == null)
                CacheDocument(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml((string)HttpRuntime.Cache[url]);
            return document;
            
        }

        private void CacheDocument(string url)
        {
            // Download data
            WebClient client = new WebClient();
            var data = client.DownloadData(url);
            var html = Encoding.UTF8.GetString(data);

            HttpRuntime.Cache.Insert(url, html, null, DateTime.Now.AddDays(1), Cache.NoSlidingExpiration);
        }

        public HtmlDocument GetLeaguesDocument()
        {
            return GetDocumentCache(LEAGUES_URL);
        }

        public List<League> GetLeagues(HtmlDocument document)
        {
            List<League> leagues = new List<League>();
            if (document.DocumentNode != null)
            {
                List<HtmlNode> nodes = document.DocumentNode.Descendants("a").Where(d =>
                    d.ParentNode.ParentNode.ParentNode.Attributes.Contains("class") &&
                    d.ParentNode.ParentNode.ParentNode.Attributes["class"].Value.Contains("standard") &&
                    d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("bold") &&

                    // KNBB
                    d.ParentNode != null &&
                    d.ParentNode.ParentNode != null &&
                    d.ParentNode.ParentNode.ChildNodes.Count >= 2 &&
                    d.ParentNode.ParentNode.ChildNodes[1].ChildNodes.Count >= 1 &&
                    d.ParentNode.ParentNode.ChildNodes[1].ChildNodes[0].Attributes.Contains("href") &&
                    d.ParentNode.ParentNode.ChildNodes[1].ChildNodes[0].Attributes["href"].Value.Contains("KNBB")
                ).ToList();

                for (var i = 0; i < nodes.Count(); i++)
                {
                    var node = nodes[i];
                    var leagueName = node.InnerText;
                    var href_split = node.Attributes["href"].Value.Split('/');
                    var leagueId = href_split[href_split.Count() - 1];
                    leagues.Add(new League() { ID = leagueId, Name = leagueName });
                }
            }
            return leagues;
        }

        public HtmlDocument GetLeagueDocument(string leagueId)
        {
            return GetDocumentCache(string.Format(LEAGUE_URL, leagueId.Replace("/", "")));
        }

        public List<string> GetTeams(HtmlDocument document)
        {
            List<string> teams = new List<string>();
            if (document.DocumentNode != null)
            {
                List<HtmlNode> nodes = document.DocumentNode.Descendants("span").Where(d => d.ParentNode.ParentNode.ParentNode.ParentNode.Id.Equals("standingTableRR") && d.Attributes.Contains("class") && d.Attributes["class"].Value.Trim().Equals("name") && d.InnerText.Trim() != "").ToList();
                for (var i = 0; i < nodes.Count(); i++)
                {
                    var node = nodes[i];
                    teams.Add(node.InnerText);
                }
            }
            return teams;
        }

        public List<Appointment> GetTeamAppointments(HtmlDocument document, string inputTeam)
        {
            var appointments = new List<Appointment>();
            if (document.DocumentNode != null)
            {
                List<HtmlNode> nodes = document.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("name") && d.InnerText.Equals(inputTeam)).ToList();

                for (var i = 0; i < nodes.Count(); i++)
                {
                    var node = nodes[i];
                    var sharedParent = node.ParentNode.ParentNode;
                    if (sharedParent.ChildNodes.Count > 7 && sharedParent.ChildNodes.Count != 25) // Ignore the rest
                    {
                        var possibleOpponent1 = sharedParent.ChildNodes[3].InnerText.Trim();
                        var possibleOpponent2 = sharedParent.ChildNodes[17].InnerText.Trim();
                        var opponent = (possibleOpponent1.Equals(inputTeam)) ? possibleOpponent2 : possibleOpponent1;
                        var venue = sharedParent.ChildNodes[19].InnerText;
                        var olddatetimestr = sharedParent.ChildNodes[21].InnerText.Replace("&nbsp;", "");
                        DateTime dateTime = StringToDateTime(olddatetimestr);
                        appointments.Add(new Appointment()
                        {
                            Name = opponent,
                            DateTime = dateTime,
                            Location = venue
                        });
                    }
                }
            }
            return appointments;
        }

        private DateTime StringToDateTime(string datestr)
        {
            if (datestr.Trim().Equals(String.Empty))
            {
                return DateTime.MinValue;
            }
            var dateArray = datestr.Replace("  ", " ").Replace(".", " ").Split(' '); // Fri, 12, Jan, 19:00
            var validDateTimeStr = dateArray[1] + ' ' + dateArray[2] + ' ' + _currentYear + ' ' + dateArray[3];
            var result = DateTime.ParseExact(validDateTimeStr.Replace("  ", " ").Replace(".", " "), "d MMM yyyy HH:mm", CultureInfo.InvariantCulture);

            if (lastDateTime != null && result.Month < lastDateTime.Month)
            {
                lastDateTime = result;
                result = result.AddYears(1);
                _currentYear++;
            }
            else
            {
                lastDateTime = result;
            }
            return result;
        }
    }
}
