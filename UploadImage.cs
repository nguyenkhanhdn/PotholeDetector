using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotholeDetector
{
    public class UploadImage
    {
        public static byte[] GetPhoto(string filePath)
        {
            FileStream stream = new FileStream(
                filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            byte[] photo = reader.ReadBytes((int)stream.Length);

            reader.Close();
            stream.Close();

            return photo;
        }
        public static void AddPothole(
        string latitude,
        string longitude,
        string location,
        byte[] photo)
        {
            string connectionString = "Server=(local)\\sqlexpress;database=PotholeDetectorApp;uid=sa;pwd=123456";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("INSERT INTO Potholes(Latitude, Longitude, location,img) Values(@latitude, @longitude, @location, @img)", connection);

                command.Parameters.AddWithValue("@latitude", latitude);
                command.Parameters.AddWithValue("@longitude",longitude);
                command.Parameters.AddWithValue("@location", location);
                command.Parameters.AddWithValue("@img", photo);

                connection.Open();
                command.ExecuteNonQuery();
            }                
        }
    }
}

