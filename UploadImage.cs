using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["pothole"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand("INSERT INTO Potholes(Latitude, Longitude, location,img) Values(@latitude, @longitude, @location, @img)", connection);

                    command.Parameters.AddWithValue("@latitude", latitude);
                    command.Parameters.AddWithValue("@longitude", longitude);
                    command.Parameters.AddWithValue("@location", location);
                    command.Parameters.AddWithValue("@img", photo);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {

                //Do nothing
            }
                         
        }

        public static void AddPothole2(
        string latitude,
        string longitude,
        string location,
        string photo)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["pothole"].ConnectionString;
                SqlConnection con = new SqlConnection(connectionString);
                
                SqlCommand cmd = new SqlCommand("addhole", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@lat", latitude);
                cmd.Parameters.AddWithValue("@long", longitude);
                cmd.Parameters.AddWithValue("@loc", location);
                cmd.Parameters.AddWithValue("@img", photo);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

