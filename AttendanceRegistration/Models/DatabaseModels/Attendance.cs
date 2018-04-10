using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceRegistration.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int Hours { get; set; }
        public Users UserId { get; set; }
        public Dates DatesId { get; set; }
    }
}
