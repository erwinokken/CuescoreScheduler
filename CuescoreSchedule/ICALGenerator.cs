using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuescoreSchedule
{
    public class ICALGenerator
    {
        public static void SaveToFile(List<Appointment> appointments)
        {
            var filename = "cuescore.ical";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            File.AppendAllText(filename, GetICALEventsContent(appointments));
        }

        public static string GetICALEventsContent(List<Appointment> appointments)
        {
            var str = "";
            appointments.ForEach(a => str += GetICALEventContent(a));
            return GetICALBaseContent(str);
        }

        private static string GetICALBaseContent(string icalcontent)
        {
            return string.Format(@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ProPool CueScoreScheduler//EN
CALSCALE:GREGORIAN
METHOD:PUBLISH{0}
END:VCALENDAR", icalcontent);
        }

        private static string GetICALEventContent(Appointment appointment)
        {
            return string.Format(@"
BEGIN:VEVENT
DTSTAMP:{0}
ORGANIZER;CN=Erwin Okken:MAILTO:info@propoolapp.com
UID:cuescore-{1}
DTSTART:{1}
DTEND:{2}
CLASS:PRIVATE
DESCRIPTION:{3} - Data from cuescore.com - Created by propoolapp.com
SUMMARY:{3}
TRANSP:OPAQUE
LOCATION:{4}
END:VEVENT", GetTimestamp(appointment.DateTime), GetTimestamp(appointment.DateTime), GetTimestamp(appointment.DateTime.AddHours(4)), appointment.Name, appointment.Location);
        }

        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddTHHmmss");
        }
    }
}
