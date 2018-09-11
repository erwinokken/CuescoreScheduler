using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuescoreSchedule
{
    class Program
    {
        const bool SHOW_ERRORS = false;

        static void Main(string[] args)
        {
            var parser = new CuescoreParser();

            var leaguesPage1 = parser.GetLeaguesDocument(1);
            var leaguesPage2 = parser.GetLeaguesDocument(2);

            var leagues = new List<League>();

            leagues.AddRange(parser.GetLeagues(leaguesPage1));
            leagues.AddRange(parser.GetLeagues(leaguesPage2));

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("For which league do you want to select a team:");
            Console.WriteLine("");
            
            for(var i=0;i<leagues.Count;i++)
            {
                Console.WriteLine((i + 1) + ". " + leagues[i].Name);
            }

            var inputLeagueNr = Convert.ToInt16(Console.ReadLine());
            var inputLeague = leagues[inputLeagueNr - 1];

            var leagueDocument = parser.GetLeagueDocument(inputLeague.ID);
            if (SHOW_ERRORS && leagueDocument.ParseErrors != null && leagueDocument.ParseErrors.Count() > 0)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("PARSE ERRORS FOUND");
                Console.WriteLine("----------------------------------------");
                foreach (var error in leagueDocument.ParseErrors)
                {
                    Console.WriteLine(" - " + error.Reason);
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("For which team do you want the schedule:");
            Console.WriteLine("");
            var teams = parser.GetTeams(leagueDocument);

            for (var i = 0; i < teams.Count(); i++)
            {
                Console.WriteLine((i + 1) + ". " + teams[i]);
            }

            var inputTeamNr = Convert.ToInt16(Console.ReadLine());

            var inputTeam = teams[inputTeamNr - 1];
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Speelschema voor " + inputTeam + ":");
            Console.WriteLine("");
            var appointments = parser.GetTeamAppointments(leagueDocument, inputTeam);
            foreach(var appointment in appointments)
            {
                Console.Write(string.Format("{0,-35}", appointment.Name) + string.Format("{0,-35}", appointment.Location) + string.Format("{0,-35}", appointment.DateTime.ToString("dd-MM-yyyy HH:mm")));
                Console.WriteLine();
            }

            ICALGenerator.SaveToFile(appointments);
            Console.WriteLine("");
            Console.WriteLine("ICAL-file saved to cuescore.ical - Press a key to close");
            Console.ReadKey();
        }
    }
}
