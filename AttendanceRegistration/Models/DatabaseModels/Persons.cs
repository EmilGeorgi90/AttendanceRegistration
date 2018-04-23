using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttendanceRegistration.Models
{
    public class Persons
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<Notes> Notes { get; set; }
    }
}
