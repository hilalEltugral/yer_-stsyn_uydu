using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Data.SqlClient;
using System.IO.Ports;
using System.IO;
using GMap.NET;
using System.Drawing.Imaging;
using System.Data.OleDb;
using System.Threading;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Net;
using System.Net.Mail;

namespace interfaceR
{
    public partial class Form1 : MaterialForm
    {
        long max = 30, min = 0;
        float pitch = 90;
        float roll = 0;
        float yaw = 0;
        float iogl;
        int width;
        int height;


        OleDbConnection baglanti = new OleDbConnection(@"Provider = Microsoft.ACE.OLEDB.12.0; Data Source =./data/veriler.xlsx; Extended Properties ='Excel 12.0 Xml;'");
        private readonly MaterialSkinManager materialSkinManager;
        private Thread thread;
        private bool runThread = false;
        private bool debugMode = false;
        public Form1()
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;//tam ekran kodu
            this.FormClosed += new FormClosedEventHandler(Form1_Closing);
            InitializeComponent();
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            // materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            OpenGlControl1.InitializeContexts();
            //OpenGl ilk işlemler
            width = OpenGlControl1.Width;
            height = OpenGlControl1.Height;
            Gl.glViewport(0, 0, width, height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(50.0f, (double)width / (double)height, 0.5f, 200.0f);

            Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_DONT_CARE);

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();  //Seri portları diziye ekleme
            foreach (string port in ports)
            {
                materialComboBox1.Items.Add(port);//Seri portları comboBox1'e ekleme
            }
             gMapAktif.MinZoom = 0;
             gMapAktif.MaxZoom = 90;
        }

       
        
        
        private void Form1_Closing(object sender, FormClosedEventArgs e)
        {
            stop();
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!debugMode)
                {
                    serialPort1.PortName = materialComboBox1.Text;
                    serialPort1.BaudRate = 9600;
                    serialPort1.Open();
                    serialPort1.DataBits = 8;
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Handshake = Handshake.None;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");    //Hata mesajı göster
            }
        }

        void degisim(float pitch, float yaw, float roll)
        {
            Gl.glTranslated(0, 0, -60);
            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glClearDepth(6f);
            Gl.glLineWidth(4);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3f(1, 0, 0);

            Gl.glVertex3f(-30, -30, -30);
            Gl.glVertex3f(80, -30, -30);

            Gl.glColor3f(0, 1, 0);
            Gl.glVertex3f(-30, -30, -30);
            Gl.glVertex3f(-30, 80, -30);


            Gl.glColor3f(0, 0, 1);
            Gl.glVertex3f(-30, -30, -30);
            Gl.glVertex3f(-30, -30, 80);

            Gl.glEnd();


            Gl.glRotatef(1, 0, 0, 1);
            Gl.glRotatef(pitch, -1, 0, 0);
            Gl.glRotatef(yaw, 0, 1, 0);
            Gl.glRotatef(roll, 0, 0, 1);
            Gl.glBegin(Gl.GL_TRIANGLES);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_POLYGON_SMOOTH);
            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_DONT_CARE);
            Gl.glBegin(Gl.GL_POLYGON);  //mulai gambar gl polygon
            for (iogl = 0; (iogl <= 359); iogl++)
            {
                //Tabung
                Gl.glColor3f(1, 0, 1);
                Gl.glVertex3f(4 * (float)Math.Sin(Math.PI / 180 * iogl), 4 * (float)Math.Cos(Math.PI / 180 * iogl), 25);
                Gl.glVertex3f(4 * (float)Math.Sin(Math.PI / 180 * (iogl + 30)), 4 * (float)Math.Cos(Math.PI / 180 * (iogl + 30)), 25);
                Gl.glColor3f(0, 0, 1);
                Gl.glVertex3f(4 * (float)Math.Sin(Math.PI / 180 * iogl), 4 * (float)Math.Cos(Math.PI / 180 * iogl), 6);
                Gl.glVertex3f(4 * (float)Math.Sin(Math.PI / 180 * (iogl + 30)), 4 * (float)Math.Cos(Math.PI / 180 * (iogl + 30)), 6);


            }
            Gl.glEnd();

        }
        private void materialButton2_Click(object sender, EventArgs e)
        {
            baglanti.Open();
            thread = new Thread(new ThreadStart(setUI));
            runThread = true;
            thread.Start();
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            Form2 yeniSayfa = new Form2();
            yeniSayfa.ShowDialog();
            this.Show();
        }

        private void stop()
        {
            runThread = false;

            if (!debugMode)
            {
                if (serialPort1.IsOpen) {
                    serialPort1.Close();
                }
            }

            baglanti.Close();
            
            if (thread != null)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }  
            }
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void materialButton6_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            file.FilterIndex = 2;
            file.RestoreDirectory = true;
            file.CheckFileExists = false;

            file.Multiselect = true;

            if (file.ShowDialog() == DialogResult.OK)
            {
                string DosyaYolu = file.FileName;
                string DosyaAdi = file.SafeFileName;
                upload(DosyaYolu, DosyaAdi);

            }
            MessageBox.Show("GÖNDERİLDİ");//bilgi
        }

        private void upload(String filePath, String fileName)
        {


            string FTPDosyaYolu = "ftp://63.33.239.182//files/" + fileName;
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(FTPDosyaYolu);

            string username = "pi";
            string password = "pi";
            request.Credentials = new NetworkCredential(username, password);

            request.UsePassive = false; // pasif olarak kullanabilme
            request.UseBinary = true; // aktarım binary ile olacak
            request.KeepAlive = false; // sürekli açık tutma

            request.Method = WebRequestMethods.Ftp.UploadFile; // Dosya yüklemek için bu request metodu gerekiyor
            FileStream stream = File.OpenRead(filePath);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            Stream reqStream = request.GetRequestStream(); // yükleme işini yapan kodlar
            reqStream.Write(buffer, 0, buffer.Length);
            reqStream.Close();
        }

        private void materialButton7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                    serialPort1.Write("a");
                   // System.Threading.Thread.Sleep(50);
                
            }
        }

        private void materialButton8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                    serialPort1.Write("k");
                   // System.Threading.Thread.Sleep(50);
                
            }
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                    serialPort1.Write("m");
                  //  System.Threading.Thread.Sleep(50);
                
            }
        }

        private void materialButton9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                
                    serialPort1.Write("b");
                    //  System.Threading.Thread.Sleep(50);
                
            }
        }

        private void materialButton10_Click(object sender, EventArgs e)
        {
            webView21.Reload();
        }

        private void OpenGlControl1_Load(object sender, EventArgs e)
        {

        }

        private void setUI()
        {
            while (runThread)
            {
                

               /* float x = 0.0f;
                float y = 0.0f;
                float z = 0.0f;*/
                double sicaklik = 0.0;
                double yukseklik = 0.0;
                double basinc = 0.0;
                double hiz = 0.0;
                double pil = 0.0;
                double enlem = 0.0;
                double boylam = 0.0;
                double gpsAlt = 0.0;
                double donusSayisi = 0.0;
                double uyduStatu = 0;
                double videoBilgi = 0;

                try
                {
                    string sonuc = "";

                    if (debugMode)
                    {
                        Random random = new Random();

                        sonuc =
                            random.Next(900, 1023) + ":" + // p (basınç)
                            random.Next(-10, 360) + ":" + // yükseklik
                             random.Next(-10, 100) + ":" +  // hiz
                             random.Next(20, 50) + ":" + // temp
                             random.Next(5, 12) + ":" + // pil
                              40.7405 + "" + random.Next(5, 9) + ":" + // enlem
                            30.3355 + "" + random.Next(5, 9) + ":" + // boylam
                            30.3355 + "" + random.Next(5, 7) + ":" + // gpsAlt
                             random.Next(0, 3) + ":" +//uyduStatüsü
                           90 + "" +":" +  random.Next(-3, 4) + ":" + // x
                            0 + ""+ ":" + random.Next(-3, 4) + ":" + // y
                             0 + ""+ ":" +random.Next(-3, 4) + ":" + // z                         
                            random.Next(5, 12) + ":" +//donüşSayısı
                            random.Next(0, 1) + ":";//videoBilgisi 
                            

                    }
                    else
                    {
                        sonuc = serialPort1.ReadLine();
                    }
                    Random random2 = new Random();
                    if (sonuc.Contains(":"))

                    {

                        string[] pot = sonuc.Split(':');

                        basinc = Convert.ToDouble(pot[0].Replace('.', ','));
                        yukseklik = Convert.ToDouble(pot[1].Replace('.', ','));
                        hiz = Double.Parse(pot[2].Replace('.', ','));
                        sicaklik = Double.Parse(pot[3].Replace('.', ','));
                        pil = Convert.ToDouble(pot[4].Replace('.', ','));
                        enlem = Convert.ToDouble(pot[5].Replace('.', ',')) + random2.Next(5, 9);
                        boylam = Convert.ToDouble(pot[6].Replace('.', ',')) + random2.Next(5, 9);
                        gpsAlt = Convert.ToDouble(pot[7].Replace('.', ','));
                        uyduStatu= Convert.ToDouble(pot[8].Replace('.', ','));
                        pitch = (float)Convert.ToDouble(pot[9])+347f ;
                        roll = (float)Convert.ToDouble(pot[10])-130f;
                        yaw = (float)Convert.ToDouble(pot[11]);
                        donusSayisi= Convert.ToDouble(pot[12].Replace('.', ','));
                        videoBilgi = Convert.ToDouble(pot[13].Replace('.', ','));


                    }
                }
                catch (Exception)
                {

                }

                if (runThread)
                {
                    materialTextBox10.Invoke((MethodInvoker)delegate {
                        // Running on the UI thread
                        materialTextBox10.Text = DateTime.Now.ToString();
                        materialTextBox2.Text = "50382";
                    });

                    chart1.Invoke((MethodInvoker)delegate {
                        chart1.ChartAreas[0].AxisX.Minimum = min;
                        chart1.ChartAreas[0].AxisX.Maximum = max;

                        chart1.ChartAreas[0].AxisY.Maximum = Double.NaN;

                        this.chart1.Series[0].Points.AddXY(max, basinc);
                        chart1.Series[0].IsVisibleInLegend = false;// remove legend
                        chart1.Series["Pa"].BorderWidth = 2;
                    });

                    chart2.Invoke((MethodInvoker)delegate {
                        chart2.ChartAreas[0].AxisX.Minimum = min;
                        chart2.ChartAreas[0].AxisX.Maximum = max;

                        chart2.ChartAreas[0].RecalculateAxesScale();

                        this.chart2.Series[0].Points.AddXY(max, hiz);
                        chart2.Series[0].IsVisibleInLegend = false;// remove legend
                        chart2.Series["v"].BorderWidth = 2;
                    });

                    chart3.Invoke((MethodInvoker)delegate {
                        chart3.ChartAreas[0].AxisX.Minimum = min;
                        chart3.ChartAreas[0].AxisX.Maximum = max;

                        chart3.ChartAreas[0].RecalculateAxesScale();

                        this.chart3.Series[0].Points.AddXY(max, pil);
                        chart3.Series[0].IsVisibleInLegend = false;// remove legend
                        chart3.Series["V"].BorderWidth = 2;
                    });

                    chart4.Invoke((MethodInvoker)delegate {
                        chart4.ChartAreas[0].AxisX.Minimum = min;
                        chart4.ChartAreas[0].AxisX.Maximum = max;

                        chart4.ChartAreas[0].RecalculateAxesScale();

                        this.chart4.Series[0].Points.AddXY(max, sicaklik);
                        chart4.Series[0].IsVisibleInLegend = false;// remove legend
                        chart4.Series["°C"].BorderWidth = 2;
                    });

                    chart5.Invoke((MethodInvoker)delegate {
                        chart5.ChartAreas[0].AxisX.Minimum = min;
                        chart5.ChartAreas[0].AxisX.Maximum = max;

                        chart5.ChartAreas[0].RecalculateAxesScale();

                        this.chart5.Series[0].Points.AddXY(max, yukseklik);
                        chart5.Series[0].IsVisibleInLegend = false;// remove legend
                        chart5.Series["m"].BorderWidth = 2;
                    });

                    OpenGlControl1.Invoke((MethodInvoker)delegate {

                        degisim(pitch, roll, yaw);
                        Refresh();
                    });
                    
                    materialTextBox10.Invoke((MethodInvoker)delegate {
                        // Running on the UI thread
                        materialTextBox13.Text = pitch + "";
                        materialTextBox12.Text = roll + "";
                        materialTextBox11.Text = yaw + "";
                        materialTextBox5.Text = sicaklik + "";
                        materialTextBox1.Text = basinc + "";
                        materialTextBox4.Text = yukseklik + "";
                        materialTextBox3.Text = hiz + "";
                        materialTextBox6.Text = pil + "";
                        materialTextBox9.Text = enlem + "";
                        materialTextBox8.Text = boylam + "";
                        materialTextBox15.Text = donusSayisi + "";
                        materialTextBox7.Text = gpsAlt + "";
                       


                    });

                }

                min++;
                max++;

                if (!debugMode)
                {
                    serialPort1.DiscardInBuffer();
                }

                //execele id basma 
                OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [Sayfa1$]", baglanti);
                DataTable dt = new DataTable();
                dt.Clear();
                da.Fill(dt);
                dataGridView2.DataSource = dt;
                //bit

                sayac1.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    sayac1.Text = dataGridView2.RowCount.ToString();

                });

                //excel kayıt alma
                OleDbCommand komut = new OleDbCommand("insert into[Sayfa1$](TAKIMNO,PAKETNUMARASI,GÖNDERMESAATİ,BASINÇ,YÜKSEKLİK,İNİŞHIZI,SICAKLIK,PİLGERİLİMİ,GPSLATİTUDE,GPSLONGİTUDE,PİTCH,ROLL) values (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12)", baglanti);

                materialTextBox2.Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    komut.Parameters.AddWithValue("@p1", materialTextBox2.Text);
                    komut.Parameters.AddWithValue("@p2", sayac1.Text);
                    komut.Parameters.AddWithValue("@p3", materialTextBox10.Text);
                    komut.Parameters.AddWithValue("@p4", materialTextBox1.Text);
                    komut.Parameters.AddWithValue("@p5", materialTextBox4.Text);
                    komut.Parameters.AddWithValue("@p6", materialTextBox3.Text);
                    komut.Parameters.AddWithValue("@p7", materialTextBox5.Text);
                    komut.Parameters.AddWithValue("@p8", materialTextBox6.Text);
                    komut.Parameters.AddWithValue("@p9", materialTextBox9.Text);
                    komut.Parameters.AddWithValue("@p10", materialTextBox8.Text);
                    komut.Parameters.AddWithValue("@p11", materialTextBox13.Text);
                    komut.Parameters.AddWithValue("@p12", materialTextBox12.Text);
                });

                komut.ExecuteNonQuery();

                // Harita gösterme
                  gMapAktif.Invoke((MethodInvoker)delegate {
                      // Running on the UI thread
                      gMapAktif.MapProvider = GMap.NET.MapProviders.ArcGIS_StreetMap_World_2D_MapProvider.Instance;
                      GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
                      gMapAktif.Position = new GMap.NET.PointLatLng(enlem, boylam);
                  });
                
                Thread.Sleep(1000);
            }
        }

    }
}



/*
 * DialogResult result = MessageBox.Show("Do you really want to exit?", "Dialog Title", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes)
        {
            Environment.Exit(0);
        }
        else
        {
            e.Cancel = true;
        }
 */

