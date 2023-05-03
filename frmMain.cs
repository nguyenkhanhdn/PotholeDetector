using AForge.Video;
using AForge.Video.DirectShow;
using lobe;
using lobe.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PotholeDetector
{
    public partial class frmMain : Form
    {
        private GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

        private GeoCoordinateWatcher _geoWatcher;

        private VideoCaptureDevice captureDevice;
        private FilterInfoCollection filterInfo;
        void StartCamera()
        {
            try
            {
                filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                captureDevice = new VideoCaptureDevice(filterInfo[0].MonikerString);

                captureDevice.NewFrame += new NewFrameEventHandler(CameraOn);
                captureDevice.Start();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void CameraOn(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        public frmMain()
        {
            InitializeComponent();
            _geoWatcher = new GeoCoordinateWatcher();
            _geoWatcher.StatusChanged += GeoWatcherOnStatusChanged;
        }

        private void GeoWatcherOnStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status != GeoPositionStatus.Ready) return;

            GeoPositionPermission allowed = _geoWatcher.Permission;

            GeoPosition<GeoCoordinate> coordinate = _geoWatcher.Position;

            GeoCoordinate deviceLocation = coordinate.Location;
            DateTimeOffset fetchedAt = coordinate.Timestamp;
            string url = string.Format("Lat: {0}, Long: {1}, fetched at: {2}",
                deviceLocation.Latitude,
                deviceLocation.Longitude,
                fetchedAt.DateTime.ToShortTimeString());

            label2.Text = url;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string GetGrantParentDir()
        {
            string path = Environment.CurrentDirectory;
            string fullpath = System.IO.Path.Combine(path, @"..\..");
            return fullpath;
        }

        private void UploadFile(string filename)
        {
            try
            {
                var wc = new WebClient();
                byte[] response = wc.UploadFile("https://localhost:44348/Home/UploadFile", "POST", filename);
                string s = System.Text.Encoding.ASCII.GetString(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Upload file: " + ex.Message);
            }                        
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string parentDir = Environment.CurrentDirectory;

                string imageDir = Path.Combine(parentDir,"holes");
                var bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.DrawToBitmap(bitmap, pictureBox1.ClientRectangle);
                byte[] bytes = ImageToByte2(bitmap);
                string className = Predict(bytes);
                
                ProcessResult(className);

                if ((className == "huhai") || (className == "xuocnhe"))
                {
                    //Save image for upload to website
                    ImageFormat imageFormat = imageFormat = ImageFormat.Jpeg;
                    string filename = "pothole" + DateTime.Now.ToLongTimeString().GetHashCode().ToString() + ".jpg";
                    string fullpath = System.IO.Path.Combine(imageDir,filename);
                    MessageBox.Show("Save image.");
                    bitmap.Save(fullpath, imageFormat);
                    

                    //Get location where pothole was detected
                    var whereat = watcher2.Position.Location;
                    var Lat = whereat.Latitude.ToString("0.00000000");
                    var Lon = whereat.Longitude.ToString("0.00000000");
                    //Format for map location
                    string loc = string.Format("https://maps.google.com/?q={0},{1}", Lat, Lon);

                    //optional parameters for future use
                    whereat.Altitude.ToString();
                    whereat.HorizontalAccuracy.ToString();
                    whereat.VerticalAccuracy.ToString();
                    whereat.Course.ToString();
                    whereat.Speed.ToString();
                    MessageBox.Show("upload image");
                    //Call upload file
                    UploadFile(fullpath);
                    MessageBox.Show("Add to database");
                    UploadImage.AddPothole2(Lat, Lon, loc, filename);                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Timer error: " + ex.Message);
            }
        }
        private void ProcessResult(string level)
        {
            this.label1.Text = level;

            #region process
            //SoundPlayer simpleSound = null;
            //if (level == "level 1")
            //{
            //    textBox1.Text = "Cấp độ cảnh báo: cấp độ 1";
            //    textBox2.Text = "Bình thường";
            //    textBox3.Text = "Cảnh báo: TẮT";
            //    if (simpleSound != null)
            //    {
            //        simpleSound.Stop();
            //    }
            //    SendToCloud(1, textBox1.Text, textBox2.Text);
            //}
            //else if (level == "level 2")
            //{
            //    textBox1.Text = "Cấp độ cảnh báo: cấp độ 2";
            //    textBox2.Text = "Sương mù";
            //    textBox3.Text = "Cảnh báo: BẬT";

            //    var soundFile = Environment.CurrentDirectory + @"\sound\alarm.wav";
            //    simpleSound = new SoundPlayer(soundFile);
            //    simpleSound.Play();

            //    SendToCloud(2, textBox1.Text, textBox2.Text);
            //}
            //else if (level == "level 3")
            //{
            //    textBox1.Text = "Cấp độ cảnh báo: cấp độ 3";
            //    textBox2.Text = "Cháy rừng";
            //    textBox3.Text = "Cảnh báo: BẬT";
            //    var soundFile = Environment.CurrentDirectory + @"\sound\alarm2.wav";
            //    simpleSound = new SoundPlayer(soundFile);
            //    simpleSound.Play();
            //    SendToCloud(3, textBox1.Text, textBox2.Text);
            //}
            //else
            //{
            //    textBox1.Text = "Cấp độ cảnh báo: cấp độ 0";
            //    textBox2.Text = "Bình thường";
            //    textBox3.Text = "Cảnh báo: TẮT";
            //    if (simpleSound != null)
            //    {
            //        simpleSound.Stop();
            //    }
            //    SendToCloud(0, "Cấp độ cảnh báo: cấp độ 0", "Bình thường");
            //}
            #endregion
        }

        private void SendToCloud(int level, string desc, string onOff)
        {
            try
            {
                const string WRITEKEY = "TLCC0QSDXNRF07ZD";
                string strUpdateBase = "http://api.thingspeak.com/update";
                string strUpdateURI = strUpdateBase + "?key=" + WRITEKEY;
                string strField1 = level.ToString();
                string strField2 = desc;
                string strField3 = onOff;
                HttpWebRequest ThingsSpeakReq;
                HttpWebResponse ThingsSpeakResp;

                strUpdateURI += "&field1=" + strField1;
                strUpdateURI += "&field2=" + strField2;
                strUpdateURI += "&field3=" + strField3;

                ThingsSpeakReq = (HttpWebRequest)WebRequest.Create(strUpdateURI);

                ThingsSpeakResp = (HttpWebResponse)ThingsSpeakReq.GetResponse();

                if (!(string.Equals(ThingsSpeakResp.StatusDescription, "OK")))
                {
                    Exception exData = new Exception(ThingsSpeakResp.StatusDescription);
                    //throw exData;
                }
            }
            catch (Exception ex)
            {
                this.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private string Predict(string fileName)
        {
            //var signatureFilePath = Environment.CurrentDirectory + @"\model\signature.json";

            var signatureFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\model\signature.json");
            var imageToClassify = fileName;

            lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
            var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            //var classifier2 = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(imageToClassify).CloneAs<Rgb24>());
            return results.Prediction.Label;
        }

        private string Predict(byte[] bitmap)
        {
            //var signatureFilePath = Environment.CurrentDirectory + @"\model\signature.json";
            var signatureFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\model\signature.json");
            lobe.ImageClassifier.Register("onnx", () => new OnnxImageClassifier());
            var classifier = ImageClassifier.CreateFromSignatureFile(new FileInfo(signatureFilePath));

            var results = classifier.Classify(SixLabors.ImageSharp.Image.Load(bitmap).CloneAs<Rgb24>());
            return results.Prediction.Label;
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            StartCamera();
            watcher2.Start();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            captureDevice.Stop();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ribbonButton2_Click(object sender, EventArgs e)
        {
            //UploadImage.AddPothole2("1001010", "10101001", "112121212", "abc.jpog");
        }
        private void Watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e) // Find GeoLocation of Device  
        {
            string latitude = "0";
            string longitute = "0";
            try
            {
                if (e.Status == GeoPositionStatus.Ready)
                {
                    // Display the latitude and longitude.  
                    if (watcher.Position.Location.IsUnknown)
                    {
                        latitude = "0";
                        longitute = "0";
                    }
                    else
                    {
                        latitude = watcher.Position.Location.Latitude.ToString();
                        longitute = watcher.Position.Location.Longitude.ToString();
                    }
                }
                else
                {
                    latitude = "0";
                    longitute = "0";
                }
            }
            catch (Exception)
            {
                latitude = "0";
                longitute = "0";
            }
            MessageBox.Show(string.Format("{0},{1}", latitude, longitute));
        }
        private GeoCoordinateWatcher watcher2 = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        private void ribbonButton4_Click(object sender, EventArgs e)
        {
            LocationMessage();
        }

        private string LocationMessage()
        {

            //https://www.google.com/maps/@16.0749383,108.2258899,15z

            var whereat = watcher2.Position.Location;
            
            var Lat = whereat.Latitude.ToString("0.000000000");
            var Lon = whereat.Longitude.ToString("0.000000000");

            string loc = string.Format("https://www.google.com/maps/@{0},{1}z",Lat,Lon);

            //optional parameters for future use
            whereat.Altitude.ToString();
            whereat.HorizontalAccuracy.ToString();
            whereat.VerticalAccuracy.ToString();
            whereat.Course.ToString();
            whereat.Speed.ToString();
            //MessageBox.Show(loc);
            return loc;
        }

        private void ribbonButton3_Click(object sender, EventArgs e)
        {
            
            String uriString = "http://localhost/pothole/uploads";

            // Create a new WebClient instance.
            WebClient myWebClient = new WebClient();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PNG files (*.png)|*.png|Jpeg files (*.jpg)|*.jpg|All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                byte[] responseArray = myWebClient.UploadFile(uriString, ofd.FileName);
                MessageBox.Show(System.Text.Encoding.ASCII.GetString(responseArray));
            }
            // Upload the file to the URI.
            // The 'UploadFile(uriString,fileName)' method implicitly uses HTTP POST method.

            else
            {
                MessageBox.Show("Error");
            }
        }
    }
}
