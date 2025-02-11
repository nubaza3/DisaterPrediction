using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using disaterprediction.Models;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using System.ComponentModel;
using System.Net.Http;

namespace disaterprediction.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public ActionResult Index()
        {
            return View();
        }


        public JsonResult Region(List<RegionModel> region)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Disater"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    foreach (var i in region)
                    {
                        foreach (var t in i.DisaterTypes)
                        {
                            using (var cmd = new SQLiteCommand("SELECT 1 FROM Region WHERE RegionId = @RegionId AND Disatertype = @Disatertype", conn))
                            {
                                cmd.Parameters.AddWithValue("@RegionId", i.RegionID);
                                cmd.Parameters.AddWithValue("@Disatertype", t);
                                var checkdisater = cmd.ExecuteScalar();
                                if (checkdisater == null)
                                {
                                    using (var insert = new SQLiteCommand("INSERT INTO Region (RegionId, Latitude, Longtitude, Disatertype) VALUES (@RegionId, @Latitude, @Longtitude, @Disatertype)", conn))
                                    {
                                        insert.Parameters.AddWithValue("@RegionId", i.RegionID);
                                        insert.Parameters.AddWithValue("@Latitude", i.LocationCoordinates.latitude);
                                        insert.Parameters.AddWithValue("@Longtitude", i.LocationCoordinates.longtitude);
                                        insert.Parameters.AddWithValue("@Disatertype", t);
                                        insert.ExecuteNonQuery(); 
                                    }
                                }
                                else
                                {
                                    using (var update = new SQLiteCommand("UPDATE Region SET Latitude = @Latitude, Longtitude = @Longtitude WHERE RegionId = @RegionId AND Disatertype = @Disatertype", conn))
                                    {
                                        update.Parameters.AddWithValue("@Latitude", i.LocationCoordinates.latitude);
                                        update.Parameters.AddWithValue("@Longtitude", i.LocationCoordinates.longtitude);
                                        update.Parameters.AddWithValue("@RegionId", i.RegionID);
                                        update.Parameters.AddWithValue("@Disatertype", t);
                                        update.ExecuteNonQuery(); 
                                    }
                                }
                            }
                        }
                    }
                }

                return Json(new
                {
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });
            }
        }


        public JsonResult AlertSetting(List<AlertSetting> AlertSetting)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Disater"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    foreach (var i in AlertSetting)
                    {
                        using (SQLiteCommand checkcmd = new SQLiteCommand("SELECT 1 FROM AlertSetting WHERE RegionId = @RegionId AND Disatertype = @Disatertype", conn))
                        {
                            checkcmd.Parameters.AddWithValue("@RegionId", i.RegionID);
                            checkcmd.Parameters.AddWithValue("@Disatertype", i.DisaterTypes);
                            var result = checkcmd.ExecuteScalar();
                            if (result == null)
                            {
                                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO AlertSetting (RegionId, Disatertype, ThresholdScore) VALUES (@RegionId, @Disatertype, @ThresholdScore)", conn))
                                {
                                    cmd.Parameters.AddWithValue("@RegionId", i.RegionID);
                                    cmd.Parameters.AddWithValue("@Disatertype", i.DisaterTypes);
                                    cmd.Parameters.AddWithValue("@ThresholdScore", i.ThresholdScore);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SQLiteCommand cmd = new SQLiteCommand("UPDATE AlertSetting SET ThresholdScore = @ThresholdScore WHERE RegionId = @RegionId AND Disatertype = @Disatertype", conn))
                                {
                                    cmd.Parameters.AddWithValue("@RegionId", i.RegionID);
                                    cmd.Parameters.AddWithValue("@Disatertype", i.DisaterTypes);
                                    cmd.Parameters.AddWithValue("@ThresholdScore", i.ThresholdScore);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                return Json(new
                {
                    IsSuccess = true,
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        public JsonResult DisaterRiskReport()
        {
            try
            {

                var disaterrisk = new List<DisaterRisk>();
                string connectionString = ConfigurationManager.ConnectionStrings["Disater"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    using (var cmd = new SQLiteCommand("select RegionId , DisaterType, RiskScore, RiskLevel,AlertTriggered from DisaterRiskReport", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var data = new DisaterRisk();

                                    data.RegionID = reader.GetString(0);
                                    data.DisaterTypes = reader.GetString(1);
                                    data.RiskScore = reader.GetInt32(2);
                                    data.RiskLevel = reader.GetString(3);
                                    data.AlertTriggered = reader.GetString(4);


                                    disaterrisk.Add(data);

                                }
                            }
                        }
                        else
                        {
                            throw new Exception("no values");
                        }
                    }
                }
                return Json(new
                {
                    IsSuccess = true,
                    disaterrisk
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });
            }
        }

    }
}