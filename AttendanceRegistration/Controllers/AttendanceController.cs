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
                    Notes note = new Notes();
                    Users user = new Users();
                    Dates date = new Dates();
                    command.CommandText = "EXECUTE Data";
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    List<PropertyInfo[]> props = new List<PropertyInfo[]>();
                    PropertyInfo[] attendanceProps = typeof(Attendance).GetProperties();
                    PropertyInfo[] datesProps = typeof(Dates).GetProperties();
                    PropertyInfo[] usersProp = typeof(Users).GetProperties();
                    PropertyInfo[] notesProp = typeof(Notes).GetProperties();
                    props.Add(attendanceProps);
                    props.Add(datesProps);
                    props.Add(usersProp);
                    props.Add(notesProp);
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        foreach (DataRow dr in ds.Tables[i].Rows)
                        {
                            date = new Dates() { DatesId = (int)dr[datesProps[0].Name], ShcoolData = (DateTime)dr[datesProps[1].Name] };
                            user = new Users() { UserId = (int)dr[usersProp[0].Name], Username = (string)dr[usersProp[1].Name], Fullname = (string)dr[usersProp[2].Name], Phonenumber = (string)dr[usersProp[3].Name], Email = (string)dr[usersProp[4].Name] };
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

                }
            }
            catch
            {
                throw;
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
        public IActionResult Create(string notes, int attendanceId)
        {
            try
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                using (SqlCommand command = new SqlCommand() { Connection = conn, Transaction = trans })
                {
                    try
                    {
                        command.CommandText = $"INSERT INTO Notes (note, notesid) VALUES ({notes}, {attendanceId}";
                        command.ExecuteNonQuery();
                        trans.Commit();
                    }
                    catch (SqlException)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
            finally
            {
                conn.Close();
            }
        }

        // GET: Attendance/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: Attendance/Edit/5
        [HttpPost]
        public IActionResult Edit(IList<Persons> ad)
        {
            try
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.Transaction = trans;
                    try
                    {
                        foreach (Persons item in ad)
                        {
                            for (int i = 0; i < item.Attendances.Count; i++)
                            {

                                command.CommandText = $"UPDATE Attendance SET hours = {item.Attendances[i].Hours} WHERE AttendanceId = {item.Attendances[i].AttendanceId}";
                                command.ExecuteNonQuery();
                                if (!(item.Notes is null))
                                {
                                    command.CommandText = $"UPDATE notes SET note = '{item.Notes[i].Note}' WHERE notesId = {item.Notes[i].NotesId}";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        trans.Commit();
                    }
                    catch (SqlException)
                    {
                        trans.Rollback();
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