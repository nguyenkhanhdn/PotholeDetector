using System;
using System.Collections.Generic;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PotholeDetector
{
    public class PotholeUtils
    {
        public static string GetLoc()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            string loc = "Unknown";
            watcher.PositionChanged += (sender, e) =>
            {
                var coordinate = e.Position.Location;
                loc = string.Format("Lat: {0}, Long: {1}", coordinate.Latitude,coordinate.Longitude);
                // Uncomment to get only one event.
                // watcher.Stop();
            };

            // Begin listening for location updates.
            watcher.Start();
            return loc;
        }
        public static string GetLocation()
        {
            string loc = "";
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

            // Do not suppress prompt, and wait 1000 milliseconds to start.
            watcher.TryStart(true, TimeSpan.FromMilliseconds(10000));

            GeoCoordinate coord = watcher.Position.Location;

            if (coord.IsUnknown != true)
            {
                //loc = string.Format("Lat: {0}, Long: {1}", coord.Latitude, coord.Longitude);
                loc = string.Format("https://www.google.com/maps/@{0},{1}", coord.Latitude, coord.Longitude);
            }
            else
            {
                loc = string.Format("Unknown latitude and longitude");
            }
            return loc;
        }
        public void UploadLoc()
        {
            Console.Write("\nPlease enter the URI to post data to : ");
            String uriString = Console.ReadLine();

            // Create a new WebClient instance.
            WebClient myWebClient = new WebClient();

            Console.WriteLine("\nPlease enter the fully qualified path of the file to be uploaded to the URI");
            string fileName = Console.ReadLine();
            Console.WriteLine("Uploading {0} to {1} ...", fileName, uriString);

            // Upload the file to the URI.
            // The 'UploadFile(uriString,fileName)' method implicitly uses HTTP POST method.
            byte[] responseArray = myWebClient.UploadFile(uriString, fileName);

            // Decode and display the response.
            Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}",
                System.Text.Encoding.ASCII.GetString(responseArray));
        }
        public static void PerisitImage(string path, IDbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                Image img = Image.FromFile(path);
                MemoryStream tmpStream = new MemoryStream();
                img.Save(tmpStream, ImageFormat.Png); // change to other format
                tmpStream.Seek(0, SeekOrigin.Begin);
                byte[] imgBytes = new byte[8192];
                tmpStream.Read(imgBytes, 0, 8192);

                command.CommandText = "INSERT INTO images(payload) VALUES (:payload)";
                IDataParameter par = command.CreateParameter();
                par.ParameterName = "payload";
                par.DbType = DbType.Binary;
                par.Value = imgBytes;
                command.Parameters.Add(par);
                command.ExecuteNonQuery();
            }
        }
    }
}
