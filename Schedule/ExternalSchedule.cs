using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentScheduler;
using System.Net.Http;
using System.Configuration;
using System.Data.SQLite;
using disaterprediction.Models;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;

namespace disaterprediction.Schedule
{
    public class ExternalSchedule : Registry
    {
        public ExternalSchedule()
        {
            Schedule<External>().ToRunNow().AndEvery(5).Minutes();
        }
       
    }
}
public class External : IJob
{
    public void Execute()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["Disater"].ConnectionString;
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand(@"select r.RegionId,r.Latitude,r.Longtitude,r.DisaterType,a.ThresholdScore FROM Region r
                                                left join AlertSetting a ON r.RegionId = a.RegionId and r.Disatertype = a.Disatertype where a.ThresholdScore is not null", conn))
            {
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var regionId = reader.GetString(0);
                            var latitude = reader.GetDouble(1);
                            var longitude = reader.GetDouble(2);
                            var DisaterType = reader.GetString(3);
                            var ThresholdScore = reader.GetInt32(4);

                            var api = "https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&eventtype=earthquake&minlatitude="+ latitude + "&minlongitude=" + longitude + "&maxlatitude=" + latitude + "&maxlongitude=" + longitude ;

                            HttpClient client = new HttpClient();


                            HttpResponseMessage response = client.GetAsync(api).Result;


                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = response.Content.ReadAsByteArrayAsync();
                                var jsonString = response.Content.ReadAsStringAsync().Result;
                                var retData = Newtonsoft.Json.JsonConvert.DeserializeObject<responseAPI>(jsonString);

                                if (retData.features.Count() > 0)
                                {


                                    foreach (var i in retData.features)
                                    {
                                        using (var insert = new SQLiteCommand("INSERT INTO DisaterLog (RegionId, RiskScore, DisaterType, TimeStamp) VALUES (@RegionId, @RiskScore, @DisaterType, @TimeStamp)", conn))
                                        {
                                            insert.Parameters.AddWithValue("@RegionId", regionId);
                                            insert.Parameters.AddWithValue("@RiskScore", i.properties.sig);
                                            insert.Parameters.AddWithValue("@DisaterType", DisaterType);
                                            insert.Parameters.AddWithValue("@TimeStamp", DateTimeOffset.FromUnixTimeMilliseconds(i.properties.time).LocalDateTime);
                                            insert.ExecuteNonQuery();
                                        }

                                        using (SQLiteCommand checkreport = new SQLiteCommand("SELECT 1 FROM DisaterRiskReport WHERE RegionId = @RegionId AND Disatertype = @DisaterType", conn))
                                        {
                                            checkreport.Parameters.AddWithValue("@RegionId", regionId);
                                            checkreport.Parameters.AddWithValue("@Disatertype", DisaterType);
                                            var checkreportresult = checkreport.ExecuteScalar();
                                            if (checkreportresult == null)
                                            {
                                                using (SQLiteCommand report = new SQLiteCommand("INSERT INTO DisaterRiskReport (RegionId, Disatertype, RiskScore,RiskLevel,AlertTriggered) VALUES (@RegionId, @Disatertype, @RiskScore,@RiskLevel,@AlertTriggered)", conn))
                                                {
                                                    report.Parameters.AddWithValue("@RegionId", regionId);
                                                    report.Parameters.AddWithValue("@Disatertype", DisaterType);
                                                    report.Parameters.AddWithValue("@RiskScore", i.properties.sig);
                                                    report.Parameters.AddWithValue("@RiskLevel", i.properties.sig >=  ThresholdScore ? "High" : "Medium" );
                                                    report.Parameters.AddWithValue("@AlertTriggered", i.properties.sig >= ThresholdScore ? "true" : "false");
                                                    report.ExecuteNonQuery();
                                                }
                                            }
                                            else
                                            {
                                                using (SQLiteCommand report = new SQLiteCommand("UPDATE DisaterRiskReport SET RiskScore = @RiskScore,RiskLevel = @RiskLevel,AlertTriggered = @AlertTriggered  WHERE RegionId = @RegionId AND Disatertype = @Disatertype", conn))
                                                {
                                                    report.Parameters.AddWithValue("@RegionId", regionId);
                                                    report.Parameters.AddWithValue("@Disatertype", DisaterType);
                                                    report.Parameters.AddWithValue("@RiskScore", i.properties.sig);
                                                    report.Parameters.AddWithValue("@RiskLevel", i.properties.sig >= ThresholdScore ? "High" : "Medium");
                                                    report.Parameters.AddWithValue("@AlertTriggered", i.properties.sig >= ThresholdScore ? "true" : "false");
                                                    report.ExecuteNonQuery();
                                                }
                                            }
                                        }

                                        if(i.properties.sig >= ThresholdScore)
                                        {
                                            using (SQLiteCommand report = new SQLiteCommand("INSERT INTO AlertData (RegionId, Disatertype, RiskLevel,AlertMessage,Timestamp) VALUES (@RegionId, @Disatertype, @RiskLevel,@AlertMessage,@Timestamp)", conn))
                                            {
                                                report.Parameters.AddWithValue("@RegionId", regionId);
                                                report.Parameters.AddWithValue("@Disatertype", DisaterType);
                                                report.Parameters.AddWithValue("@RiskLevel", "High" );
                                                report.Parameters.AddWithValue("@AlertMessage", i.properties.title);
                                                report.Parameters.AddWithValue("@Timestamp", DateTimeOffset.FromUnixTimeMilliseconds(i.properties.time).LocalDateTime);
                                                report.ExecuteNonQuery();
                                            }

                                            //string smtpServer = "smtp.gmail.com"; 
                                            //int smtpPort = 587; // ตั้งค่า Port 587 for TLS, 465 for SSL
                                            //string senderEmail = "email@gmail.com"; // ต้นทาง
                                            //string senderPassword = "password"; 
                                            //string recipientEmail = "recipient@example.com"; // ปลายทาง

                                            //SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
                                            //smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                                            //smtp.EnableSsl = true;

                                            //MailMessage mail = new MailMessage
                                            //{
                                            //    From = new MailAddress(senderEmail),
                                            //    Subject = "Alert Diaster Risk location : " + i.properties.title,
                                            //    Body = "RiskScore : " + i.properties.sig + " Time : " + DateTimeOffset.FromUnixTimeMilliseconds(i.properties.time).LocalDateTime,
                                            //    IsBodyHtml = false
                                            //};

                                            //mail.To.Add(recipientEmail);
                                            //smtp.SendMailAsync(mail);



                                        }
                                    }
                            }
                        }
                        }
                    }
                }
            }


           
          

        }
    }
}