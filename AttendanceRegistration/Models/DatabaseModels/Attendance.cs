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
        }
        public int AttendanceId { get; set; }
        public int Hours { get; set; }
        public int UserId { get; set; }
        public int DatesId { get; set; }
        public Users User { get; set; }
        public Dates Dates { get; set; }
        public Model Models { get; set; }
        public Semester Semester { get; set; }
    }
}
