using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceRegistration.Models
{
    public class Semester
    {
        public int SemesterId { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public DateTime SemesterEndDate { get; set; }

    }
}
