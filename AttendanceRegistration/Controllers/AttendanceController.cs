using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AttendanceRegistration.Models;
using HsDbFirstRealAspNetProject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index()
        {
            List<Attendance> attendances = new List<Attendance>();
            try
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand() { Connection = conn })
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    command.CommandText = "SELECT * FROM Attendance INNER JOIN Users on Attendance.UserId = Users.UserId INNER JOIN Dates on Attendance.DatesId = Dates.DatesId";
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        attendances.Add(new Attendance() { Hours = (int)dr["hours"] });
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
            return View(PaginatedList<Attendance>.CreateAsync(attendances, 0, 20));
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
        public IActionResult Create(IFormCollection collection)
        {
            try
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                using (SqlCommand command = new SqlCommand() { Connection = conn, Transaction = trans })
                {
                    try
                    {
                        foreach (DateTime day in EachDay(DateTime.Parse($"1-1-{DateTime.Now.Year}"), DateTime.Parse($"31-12-{DateTime.Now.Year}")))
                        {
                            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "debug.txt", day.ToString("yyyy-MM-dd HH':'mm':'ss"));
                            command.CommandText = $"INSERT INTO Dates (ShcoolData) VALUES ('{day.Date.ToString("yyyy-MM-dd HH':'mm':'ss")}')";
                            command.ExecuteNonQuery();
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
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