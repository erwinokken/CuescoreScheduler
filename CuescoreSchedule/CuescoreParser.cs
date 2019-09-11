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
        private const string LEAGUES_URL = "https://cuescore.com/KNBB/tournaments?q=&d=0&season=0&s=0";
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

            var document = new HtmlDocument();
            document.LoadHtml((string)HttpRuntime.Cache[url]);
            return document;
        }

        private void CacheDocument(string url)
        {
            // Download data
            var client = new WebClient();
            var data = client.DownloadData(url);
            var html = Encoding.UTF8.GetString(data);

            HttpRuntime.Cache.Insert(url, html, null, DateTime.Now.AddDays(1), Cache.NoSlidingExpiration);
        }

        public List<League> GetLeagues()
        {
            var document = GetDocumentCache(LEAGUES_URL);
            var amountOfPages = this.GetAmountOfPages(document);
            var leagues = new List<League>();
            for(var curPage = 1; curPage <= amountOfPages; curPage++)
            {
                leagues.AddRange(GetLeaguesForPage(curPage));
            }

            //if (documentNode != null)
            //{
            //    var pages = documentNode.Descendants(".pages a").Count();
            //    var tables = documentNode.Descendants("table").Where(x => x.GetAttributeValue("class", "").Contains("tournaments"));
            //    var table = tables.Last();
            //    table.AppendChild(tables.First());
                

            //}

            return leagues;
        }

        private int GetAmountOfPages(HtmlDocument document)
        {
            var documentNode = document.DocumentNode;
            if (documentNode != null)
            {
                return documentNode.SelectNodes("//span[contains(@class, 'pages')]/a").Count();
            }

            return -1;
        }

        private List<League> GetLeaguesForPage(int page)
        {
            var url = $"{LEAGUES_URL}&page={page}";
            var document = GetDocumentCache(url);
            var leagues = new List<League>();
            var documentNode = document.DocumentNode;

            if (documentNode != null)
            {
                var trs = documentNode.SelectNodes("//tr[contains(@class, 'tournament')]").ToList();
                foreach (var tr in trs)
                {
                    var thirdTd = tr.Descendants("td").ToList()[2];
                    var secondDiv = thirdTd.Descendants("div").ToList()[1];
                    var node = secondDiv.Descendants("a").First();

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
            var teams = new List<string>();
            if (document.DocumentNode != null)
            {
                var nodes = document.DocumentNode.Descendants("span").Where(d => d.ParentNode.ParentNode.ParentNode.ParentNode.Id.Equals("standingTableRR") && d.Attributes.Contains("class") && d.Attributes["class"].Value.Trim().Equals("name") && d.InnerText.Trim() != "").ToList();
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
                var originalNodes = document.DocumentNode.Descendants("tr");
                var matchNodes = originalNodes.Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("match"));
                var currentTeamMatchNodes = matchNodes.Where(x => HttpUtility.HtmlDecode(x.InnerHtml).Contains(inputTeam));

                var nodes = currentTeamMatchNodes.ToList();

                foreach (var node in nodes)
                {
                    //2, 8, 9, 10
                    var tds = node.Descendants("td").ToList();
                    var possibleOpponent1 = HttpUtility.HtmlDecode(tds[2].FirstChild.ChildNodes[1].InnerText);
                    var possibleOpponent2 = HttpUtility.HtmlDecode(tds[8].FirstChild.ChildNodes[1].InnerText);
                    var opponent = (possibleOpponent1.Equals(inputTeam)) ? possibleOpponent2 : possibleOpponent1;
                    var venue = tds[9].InnerText;
                    var dateTimeStr = tds[10].FirstChild.InnerText;
                    var relLocation = (possibleOpponent1.Equals(inputTeam)) ? "Home" : "Away";
                    var matchId = node.Attributes["id"].Value.Replace("match-", "");

                    DateTime dateTime = DateTime.MinValue;
                    DateTime.TryParse(dateTimeStr, out dateTime);
                    appointments.Add(new Appointment()
                    {
                        Name = string.Format("{0} ({1})", opponent, relLocation),
                        DateTime = dateTime,
                        Location = venue,
                        MatchId = matchId
                    });
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
