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
            List<Dates> dates = new List<Dates>();
            List<Notes> notes = new List<Notes>();
            List<Attendance> attendances = new List<Attendance>();
            List<Attendance> currentAttendance = new List<Attendance>();

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
                    command.CommandText = "EXECUTE Data";
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    command.CommandText = $"SELECT * from users where email = '{User.Identity.Name}'";
                    adapter.Fill(userdata);
                    if (userdata.Tables[0].Rows.Count <= 0)
                    {
                        command.CommandText = $"INSERT INTO users (Fullname, email) values ('{User.Identity.Name.Split('@')[0]}', '{User.Identity.Name}')";
                        command.ExecuteNonQuery();
                        command.CommandText = "SELECT * from users";
                        userdata.Clear();
                        adapter.Fill(userdata);
                    }
                    List<PropertyInfo[]> props = new List<PropertyInfo[]>();
                    PropertyInfo[] attendanceProps = typeof(Attendance).GetProperties();
                    PropertyInfo[] datesProps = typeof(Dates).GetProperties();
                    PropertyInfo[] usersProp = typeof(Users).GetProperties();
                    PropertyInfo[] notesProp = typeof(Notes).GetProperties();
                    props.Add(attendanceProps);
                    props.Add(datesProps);
                    props.Add(usersProp);
                    props.Add(notesProp);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        date = new Dates() { DatesId = (int)dr[datesProps[0].Name], ShcoolData = (DateTime)dr[datesProps[1].Name] };
                        user = new Users() { UserId = (int)dr[usersProp[0].Name], Fullname = (string)dr[usersProp[2].Name], Email = (string)dr[usersProp[4].Name] };
                        if (dr[notesProp[1].Name] is DBNull)
                        {
                        }
                        else
                        {
                            note = new Notes() { NotesId = (int)dr[notesProp[0].Name], Note = (string)dr[notesProp[1].Name] };
                        }
                        attendances.Add(new Attendance() { AttendanceId = (int)dr[attendanceProps[0].Name], Hours = (int)dr[attendanceProps[1].Name], UserId = (int)dr[attendanceProps[2].Name], DatesId = (int)dr[attendanceProps[3].Name], Dates = date, User = user });
                        if (!persons.Any(c => c.Fullname == user.Fullname))
                        {
                            date.ShcoolData = DateTime.Parse(date.ShcoolData.ToString("dd-MM-yyyy"));
                            persons.Add(new Persons() { Attendances = attendances.Where(c => c.User.Fullname == user.Fullname).ToList(), Fullname = user.Fullname, Notes = notes, Id = user.UserId });
                        }
                        else
                        {
                            Persons person = persons.Single(c => c.Fullname == user.Fullname);
                            person.Attendances.Add(attendances.Find(a => a.User.Fullname == person.Fullname));
                        }
                    }
                }
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
            ViewData["JSON"] = JsonConvert.SerializeObject(persons);
            return View(PaginatedList<Persons>.CreateAsync(persons, pageIndex ?? 1, 20));
        }

        // GET: Attendance/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: Attendance/Create
        public IActionResult Create()
        {
            return View();
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
                        int hours = 15 - DateTime.Now.Hour;
                        string date = DateTime.Now.Date.ToString("yyyy-MM-dd");
                        DateTime ndt = Convert.ToDateTime(date);

                        int userid = 0;
                        int datesId = 0;
                        int attendanceId = 0;
                        DataSet set = new DataSet();
                        DataSet ds = new DataSet();
                        command.CommandText = $"Select datesId FROM dates WHERE ShcoolData = '{date}'";
                        adapter.Fill(ds);
                        if (ds.Tables[0].Rows.Count == 0)
                        {
                            command.CommandText = $"INSERT INTO Dates ([ShcoolData]) VALUES (CONVERT(SMALLDATETIME, CONVERT(DATETIME,'{date}')))";
                            command.ExecuteNonQuery();
                        }
                        foreach (DataRow item in ds.Tables[0].Rows)
                        {
                            datesId = (int)item["datesId"];
                        }
                        DataSet user = new DataSet();
                        command.CommandText = $"SELECT userid FROM users where email = '{User.Identity.Name}'";
                        adapter.Fill(user);
                        foreach (DataRow item in user.Tables[0].Rows)
                        {
                            userid = (int)item["UserId"];
                        }
                        command.CommandText = $"Select * FROM Attendance WHERE DatesId = {datesId} AND userid = {userid}";
                        adapter.Fill(set);
                        if (set.Tables[0].Rows.Count > 0)
                        {
                            RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            command.CommandText = $"INSERT INTO attendance (hours, userid, datesId) VALUES ({hours}, {userid}, {datesId})";
                            command.ExecuteNonQuery();
                            DataSet attendance = new DataSet();
                            command.CommandText = $"SELECT AttendanceId From attendance where userid = {userid} AND datesid = {datesId}";
                            adapter.Fill(attendance);
                            foreach (DataRow item in attendance.Tables[0].Rows)
                            {
                                attendanceId = (int)item["AttendanceId"];
                            }
                            command.CommandText = $"INSERT INTO Notes (notesId, note) VALUES ({attendanceId}, null)";
                            command.ExecuteNonQuery();
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
                return RedirectToAction(nameof(Index));
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


        // GET: Attendance/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: Attendance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}