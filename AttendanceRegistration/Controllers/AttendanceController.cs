using AttendanceRegistration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace AttendanceRegistration.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        SqlConnection conn;
        public AttendanceController()
        {
            conn = new SqlConnection(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=AttendanceRegistration;Integrated Security=True");
        }
        // GET: Attendance
        public IActionResult Index(string SearchString, int? pageIndex)
        {
            ViewData["CurrentSearch"] = !string.IsNullOrWhiteSpace(SearchString) ? SearchString : "";
            List<Persons> persons = new List<Persons>();
            List<Users> users = new List<Users>();
            List<Notes> notes = new List<Notes>();
            List<Attendance> attendances = new List<Attendance>();

            try
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand() { Connection = conn })
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataSet userdata = new DataSet();
                    Semester semester = new Semester();
                    Model model = new Model();
                    Notes note = new Notes();
                    Users user = new Users();
                    Dates date = new Dates();
                    //Getting all attendance in localDB
                    //Getting Úser with the logged in Email
                    command.CommandText = $"SELECT * from users where email = '{User.Identity.Name}'";
                    adapter.Fill(userdata);
                    //if no user in the database then add the user in the database
                    if (userdata.Tables[0].Rows.Count <= 0)
                    {
                        command.CommandText = $"INSERT INTO users (Fullname, email) values ('{User.Identity.Name.Split('@')[0]}', '{User.Identity.Name}')";
                        command.ExecuteNonQuery();
                        command.CommandText = "SELECT * from users";
                        userdata.Clear();
                        adapter.Fill(userdata);
                    }
                    DataSet ds = new DataSet();
                    command.CommandText = "EXECUTE Data";
                    ds.Clear();
                    adapter.Fill(ds);
                    if (ds.Tables[0].Rows.Count < 150)
                    {
                        Create("");
                        ds.Clear();
                        command.CommandText = "EXECUTE Data";
                        adapter.Fill(ds);

                    }
                    //looping all attendance in the 'ds' dataset
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        date = new Dates() { DatesId = (int)dr["DatesId"], ShcoolData = (DateTime)dr["ShcoolData"] };
                        user = new Users() { UserId = (int)dr["UserId"], Fullname = (string)dr["Fullname"], Email = (string)dr["Email"] };
                        model = new Model() { ModelId = (int)dr["ModelsId"], ModelStartDate = (DateTime)dr["ModelStartDate"], ModelEndDate = (DateTime)dr["ModelEndDate"] };
                        semester = new Semester() { SemesterId = (int)dr["SemesterId"], SemesterStartDate = (DateTime)dr["SemesterStartDate"], SemesterEndDate = (DateTime)dr["SemesterEndDate"] };

                        //checking if notes are null becuz note is nullable
                        if (dr["Note"] is DBNull)
                        {
                        }
                        else
                        {
                            note = new Notes() { NotesId = (int)dr["NotesId"], Note = (string)dr["Note"] };
                        }
                        attendances.Add(new Attendance() { AttendanceId = (int)dr["AttendanceId"], Hours = (int)dr["Hours"], UserId = (int)dr["UserId"], DatesId = (int)dr["DatesId"], Dates = date, User = user, Semester = semester, Models = model });
                        //if no user in the list
                        if (!persons.Any(c => c.Fullname == user.Fullname))
                        {
                            persons.Add(new Persons() { Attendances = attendances.Where(c => c.User.Fullname == user.Fullname).ToList(), Fullname = user.Fullname, Notes = notes, Id = user.UserId });
                        }
                        //if a user is already in the list then only add he's attendance
                        else
                        {
                            Persons person = persons.Single(c => c.Fullname == user.Fullname);
                            person.Attendances = attendances.FindAll(a => a.User.Fullname == person.Fullname && Between(DateTime.Now, a.Semester.SemesterStartDate, a.Semester.SemesterEndDate));
                        }
                    }
                }
                //checking if it is a students
                if (User.Identity.Name.Contains("edu.campusvejle.dk"))
                {
                    persons = persons.Where(p => p.Fullname == User.Identity.Name.Split('@')[0]).ToList();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                conn.Close();
            }
            //Serialize to json for the javascript
            ViewData["JSON"] = JsonConvert.SerializeObject(persons);
            ViewData["JSONWeek"] = JsonConvert.SerializeObject(persons[0].Attendances.Where(a => a.Dates.ShcoolData.DayOfWeek == DayOfWeek.Monday && a.Dates.ShcoolData <= DateTime.Now).TakeLast(5));
            ViewData["JSONSModul"] = JsonConvert.SerializeObject(persons[0].Attendances.Where(a => Between(DateTime.Now, a.Models.ModelStartDate, a.Models.ModelEndDate)));
            ViewData["JSONSemester"] = JsonConvert.SerializeObject(persons[0].Attendances.Where(a => Between(DateTime.Now,a.Semester.SemesterStartDate,a.Semester.SemesterEndDate)));
            //adding it to the pagination
            return View(PaginatedList<Persons>.CreateAsync(persons, pageIndex ?? 1, 20));
        }


        // POST: Attendance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Create(string note)
        {
            try
            {
                using (SqlCommand command = new SqlCommand() { Connection = conn })
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    try
                    {
                        foreach (DateTime date in EachDay(DateTime.Parse("1/1/18"), DateTime.Parse("30/12/18")))
                        {

                            //the hours to add to the database '15' is the time we have off school and then - the current time you checked in
                            int hours = 0;
                            //linq if you are tolate but skill trying to check in it will go to minus and then exception is throwen becuz sql int cant be zero
                            hours = hours < 0 ? 0 : hours;
                            //setting the date
                            int userid = 0;
                            int datesId = 0;
                            int attendanceId = 0;
                            int semesterId = 0;
                            int modelId = 0;
                            DataSet set = new DataSet();
                            DataSet ds = new DataSet();
                            command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date.ToString("yyyy/MM/dd")}'";
                            adapter.Fill(ds);
                            //looking if the date does not already exists in the database
                            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);

                            // Return the week of our adjusted day
                            int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                            if (week % 2 == 1)
                            {
                                if (ds.Tables[0].Rows.Count == 0 && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Friday)
                                {
                                    command.CommandText = $"INSERT INTO Dates ([ShcoolData]) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.ToString("yyyy/MM/dd")}')))";
                                    command.ExecuteNonQuery();
                                    ds.Clear();
                                    command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date.ToString("yyyy/MM/dd")}'";
                                    adapter.Fill(ds);
                                }
                            }
                            else if (week % 2 == 0)
                            {
                                if (ds.Tables[0].Rows.Count == 0 && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    command.CommandText = $"INSERT INTO Dates ([ShcoolData]) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.ToString("yyyy/MM/dd")}')))";
                                    command.ExecuteNonQuery();
                                    ds.Clear();
                                    command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date.ToString("yyyy/MM/dd")}'";
                                    adapter.Fill(ds);
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                            DataSet user = new DataSet();
                            command.CommandText = $"SELECT userid FROM users where email = '{User.Identity.Name}'";
                            adapter.Fill(user);
                            //adding the userid to the userid int
                            foreach (DataRow item in user.Tables[0].Rows)
                            {
                                userid = (int)item["UserId"];
                            }

                            //gets the lates datesId
                            foreach (DataRow item in ds.Tables[0].Rows)
                            {
                                datesId = (int)item["datesId"];
                            }
                            command.CommandText = $"Select * FROM Attendance WHERE DatesId = {datesId} AND userid = {userid}";
                            adapter.Fill(set);
                            if (set.Tables[0].Rows.Count > 0)
                            {
                                continue;
                            }
                            DataSet models = new DataSet();
                            command.CommandText = $"SELECT * from Models";
                            adapter.Fill(models);
                            foreach (DataRow item in models.Tables[0].Rows)
                            {
                                if (Between(date, (DateTime)item["ModelStartDate"], (DateTime)item["ModelEndDate"]))
                                {
                                    modelId = (int)item["ModelsId"];
                                }
                            }
                            if (modelId == 0)
                            {
                                command.CommandText = $"INSERT INTO Models (ModelStartDate, ModelEndDate) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.ToString("yyyy/MM/dd")}')),CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.AddDays(42).ToString("yyyy/MM/dd")}')))";
                                command.ExecuteNonQuery();
                                command.CommandText = $"SELECT * from Models";
                                models.Clear();
                                adapter.Fill(models);
                                foreach (DataRow item in models.Tables[0].Rows)
                                {
                                    if (Between(date, (DateTime)item["ModelStartDate"], (DateTime)item["ModelEndDate"]))
                                    {
                                        modelId = (int)item["ModelsId"];
                                    }
                                }
                            }
                            DataSet semModels = new DataSet();
                            DataSet semester = new DataSet();
                            if (CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == 3 || CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == 33)
                            {
                                command.CommandText = $"INSERT INTO Semester (SemesterStartDate, SemesterEndDate) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.ToString("yyyy/MM/dd")}')),CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.AddDays(126).ToString("yyyy/MM/dd")}')))";
                                command.ExecuteNonQuery();
                                command.CommandText = $"SELECT * FROM Semester";
                                adapter.Fill(semester);
                                foreach (DataRow item in semester.Tables[0].Rows)
                                {
                                    if (Between(date, (DateTime)item["SemesterStartDate"], (DateTime)item["SemesterEndDate"]))
                                    {
                                        semesterId = (int)item["SemesterId"];
                                    }
                                }
                            }
                            if (semesterId == 0)
                            {
                                command.CommandText = $"SELECT * FROM Semester";
                                adapter.Fill(semester);
                                foreach (DataRow item in semester.Tables[0].Rows)
                                {
                                    if (Between(date, (DateTime)item["SemesterStartDate"], (DateTime)item["SemesterEndDate"]))
                                    {
                                        semesterId = (int)item["SemesterId"];
                                    }
                                }
                            }

                            //checking if the user already check in today

                            //if not then add the attedance

                            if (semesterId != 0 && datesId != 0 && modelId != 0 && userid != 0)
                            {

                                //insert into attendance
                                command.CommandText = $"INSERT INTO attendance (hours, userid, datesId, SemesterId, ModelId) VALUES ({hours}, {userid}, {datesId}, {semesterId}, {modelId})";
                                command.ExecuteNonQuery();
                                DataSet attendance = new DataSet();
                                //getting the id so we can add a note
                                command.CommandText = $"SELECT AttendanceId From attendance where userid = {userid} AND datesid = {datesId}";
                                adapter.Fill(attendance);
                                foreach (DataRow item in attendance.Tables[0].Rows)
                                {
                                    attendanceId = (int)item["AttendanceId"];
                                }
                                //insert the note with value null (ment to edit late)
                                //TODO: the insert adds a note and not null
                                command.CommandText = $"INSERT INTO Notes (notesId, note) VALUES ({attendanceId}, null)";
                                command.ExecuteNonQuery();
                            }

                        }

                    }
                    catch (SqlException)
                    {
                        throw;
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        // POST: Attendance/Edit/5
        [HttpPost]
        public IActionResult Edit(int hours, int attendanceId)
        {
            try
            {
                conn.Open();
                using (SqlCommand command = conn.CreateCommand())
                {
                    try
                    {
                        command.CommandText = $"UPDATE Attendance SET hours = {hours} WHERE AttendanceId = {attendanceId}";
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// getting each day
        /// </summary>
        /// <param name="from"></param>
        /// <param name="thru"></param>
        /// <returns></returns>
        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        private static bool Between(DateTime input, DateTime date1, DateTime date2)
        {
            return (input >= date1 && input <= date2);
        }
    }
}