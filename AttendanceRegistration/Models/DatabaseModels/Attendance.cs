using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceRegistration.Models
{
    public class Attendance
    {
        public Attendance()
        {
        }
        public Attendance(int id, int hours, string notes)
        {
            AttendanceId = id;
            Hours = hours;
            Notes = notes;
        }
        public int AttendanceId { get; set; }
        public int Hours { get; set; }
        public Users UserId { get; set; }
        public Dates DatesId { get; set; }
        public string Notes { get; set; }
    }
}
