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
            CuescoreParser parser = new CuescoreParser("https://cuescore.com/tournament/2017%252F2018+Pool+Noord+Eerste+Klasse/1548571");
            var document = parser.GetDocument();
            if (SHOW_ERRORS && document.ParseErrors != null && document.ParseErrors.Count() > 0)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("PARSE ERRORS FOUND");
                Console.WriteLine("----------------------------------------");
                foreach (var error in document.ParseErrors)
                {
                    Console.WriteLine(" - " + error.Reason);
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("For which team do you want the schedule:");
            Console.WriteLine("");
            var teams = parser.GetTeams(document);
            var inputTeamNr = Convert.ToInt16(Console.ReadLine());

            var inputTeam = teams[inputTeamNr - 1];
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Speelschema voor " + inputTeam + ":");
            Console.WriteLine("");
            var appointments = parser.GetTeamAppointments(document, inputTeam);
            foreach(var appointment in appointments)
            {
                Console.Write(string.Format("{0,-27}", appointment.Name) + string.Format("{0,-27}", appointment.Location) + string.Format("{0,-27}", appointment.DateTime.ToString("dd-MM-yyyy HH:mm")));
                Console.WriteLine();
            }

            ICALGenerator.SaveToFile(appointments);
            Console.WriteLine("");
            Console.WriteLine("ICAL-file saved to cuescore.ical - Press a key to close");
            Console.ReadKey();
        }
    }
}
