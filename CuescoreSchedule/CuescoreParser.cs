using System;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
 */
namespace CuescoreSchedule
{
    class CuescoreParser
    {
        private string _url;
        private DateTime lastDateTime;
        private int _currentYear;
        
        public CuescoreParser(string url)
        {
            _currentYear = DateTime.Now.Year;
            _url = url;
        }
        
        private DateTime StringToDateTime(string datestr)
        {
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

        public HtmlDocument GetDocument()
        {
            // Download data
            WebClient client = new WebClient();
            var data = client.DownloadData(_url);
            var html = Encoding.UTF8.GetString(data);

            // Parse HTML
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            return document;
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
                    Console.WriteLine((i + 1) + ". " + node.InnerText);
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
    }
}
