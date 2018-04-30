using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AttendanceRegistration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Newtonsoft.Json;

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
                    Notes note = new Notes();
                    Users user = new Users();
                    Dates date = new Dates();
                    //Getting all attendance in localDB
                    command.CommandText = "EXECUTE Data";
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
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
                    //looping all attendance in the 'ds' dataset
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        date = new Dates() { DatesId = (int)dr["DatesId"], ShcoolData = (DateTime)dr["ShcoolData"] };
                        user = new Users() { UserId = (int)dr["UserId"], Fullname = (string)dr["Fullname"], Email = (string)dr["Email"] };
                        //checking if notes are null becuz note is nullable
                        if (dr["Note"] is DBNull)
                        {
                        }
                        else
                        {
                            note = new Notes() { NotesId = (int)dr["NotesId"], Note = (string)dr["Note"] };
                        }
                        attendances.Add(new Attendance() { AttendanceId = (int)dr["AttendanceId"], Hours = (int)dr["Hours"], UserId = (int)dr["UserId"], DatesId = (int)dr["DatesId"], Dates = date, User = user });
                        //if no user in the list
                        if (!persons.Any(c => c.Fullname == user.Fullname))
                        {
                            persons.Add(new Persons() { Attendances = attendances.Where(c => c.User.Fullname == user.Fullname).ToList(), Fullname = user.Fullname, Notes = notes, Id = user.UserId });
                        }
                        //if a user is already in the list then only add he's attendance
                        else
                        {
                            Persons person = persons.Single(c => c.Fullname == user.Fullname);
                            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.Now,
                            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now.AddDays(-5)));
                            Console.WriteLine(dateTimeOffset.CompareTo(date.ShcoolData.Date) <= 5);
                            person.Attendances = attendances.FindAll(a => a.User.Fullname == person.Fullname && date.ShcoolData.Date <= DateTime.Now).TakeLast(5).ToList();
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
            //adding it to the pagination
            return View(PaginatedList<Persons>.CreateAsync(persons, pageIndex ?? 1, 20));
        }


        // POST: Attendance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string note)
        {
            try
            {
                conn.Open();
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
                            DataSet set = new DataSet();
                            DataSet ds = new DataSet();
                            command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date.ToString("yyyy/MM/dd")}'";
                            adapter.Fill(ds);
                            //looking if the date does not already exists in the database
                            if (ds.Tables[0].Rows.Count == 0)
                            {
                                command.CommandText = $"INSERT INTO Dates ([ShcoolData]) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date.ToString("yyyy/MM/dd")}')))";
                                command.ExecuteNonQuery();
                                ds.Clear();
                                command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date.ToString("yyyy/MM/dd")}'";
                                adapter.Fill(ds);
                            }
                            //gets the lates datesId
                            foreach (DataRow item in ds.Tables[0].Rows)
                            {
                                datesId = (int)item["datesId"];
                            }
                            DataSet user = new DataSet();
                            command.CommandText = $"SELECT userid FROM users where email = '{User.Identity.Name}'";
                            adapter.Fill(user);
                            //adding the userid to the userid int
                            foreach (DataRow item in user.Tables[0].Rows)
                            {
                                userid = (int)item["UserId"];
                            }
                            command.CommandText = $"Select * FROM Attendance WHERE DatesId = {datesId} AND userid = {userid}";
                            adapter.Fill(set);
                            //checking if the user already check in today
                            if (set.Tables[0].Rows.Count > 0)
                            {
                                RedirectToAction(nameof(Index));
                            }
                            //if not then add the attedance
                            else
                            {
                                //insert into attendance
                                command.CommandText = $"INSERT INTO attendance (hours, userid, datesId) VALUES ({hours}, {userid}, {datesId})";
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

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
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
    }
}