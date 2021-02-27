using System;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace CAARA
{
    public partial class Form1 : MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        //AIRPLANE Part
        bool startPoint = false;
        bool endPoint = false;

        //is the button "Start Point" pressed
        private void metroButton1_Click(object sender, EventArgs e)
        {
            endPoint = false;
            startPoint = true;
            metroLabel1.Focus();
        }

        //is the button "End Point" pressed
        private void metroButton2_Click(object sender, EventArgs e)
        {
            startPoint = false;
            endPoint = true;
            metroLabel1.Focus();
        }

        //locate Airplane or Point on map
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (startPoint == true)
            {
                pictureBox1.Controls.Add(pictureBox2);
                pictureBox2.Image = Properties.Resources.airplane_black;
                MouseEventArgs cursor = (MouseEventArgs)e;
                pictureBox2.Location = new Point(cursor.X - pictureBox2.Width / 2, cursor.Y - pictureBox2.Height / 2);
                pictureBox2.BackColor = Color.Transparent;
                startPoint = false;
            }

            if (endPoint == true)
            {
                pictureBox1.Controls.Add(pictureBox3);
                pictureBox3.Image = Properties.Resources.placeholder;
                MouseEventArgs cursor = (MouseEventArgs)e;
                pictureBox3.Location = new Point(cursor.X - pictureBox3.Width / 2, cursor.Y - pictureBox3.Width);
                pictureBox3.BackColor = Color.Transparent;
                endPoint = false;
            }
        }

        //RADAR Part
        //necessary data for radar drawing
        const int radarX = 1720;
        const int radarY = 512;

        const int width = 1600;
        const int height = 1600;
        const int hand = 800;

        int cx = width / 2 + radarX;
        int cy = height / 2 + radarY;
        int u = 0;
        int x, y;

        int tx, ty, lim = 50;

        Graphics graphics;
        Thread thread;
        bool isTurn = false;

        //is Radar turned off
        private void metroRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            try
            { 
                if (thread != null && thread.IsAlive)
                {
                    isTurn = false;
                    thread.Abort();
                    pictureBox1.Image = Properties.Resources.map;
                }
                metroLabel1.Focus();
            }catch (Exception) {}
        }

        //is Radar turned on
        private void metroRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                isTurn = true;
                thread = new Thread(Draw);
                thread.IsBackground = true;
                thread.Start();
            }catch (Exception) {}
        }

        //radar drawing function itself
        private void Draw()
        {
            while (isTurn)
            {
                try
                {
                    Bitmap bitmap = new Bitmap(width + 1, height + 1);
                    Pen greenPen = new Pen(Color.Green, 3f);
                    Pen whitePen = new Pen(Color.White, 3f);
                    graphics = Graphics.FromImage(pictureBox1.Image);

                    //calculate X, Y coordinate of HAND
                    int tu = (u - lim) % 360;

                    if (u >= 0 && u <= 180)
                    {
                        //right half
                        //u in degree is converted into radian.
                        x = cx + (int)(hand * Math.Sin(Math.PI * u / 180));
                        y = cy - (int)(hand * Math.Cos(Math.PI * u / 180));
                    }
                    else
                    {
                        //left half
                        x = cx - (int)(hand * -Math.Sin(Math.PI * u / 180));
                        y = cy - (int)(hand * Math.Cos(Math.PI * u / 180));
                    }

                    if (tu >= 0 && tu <= 180)
                    {
                        //right half
                        //tu in degree is converted into radian.
                        tx = cx + (int)(hand * Math.Sin(Math.PI * tu / 180));
                        ty = cy - (int)(hand * Math.Cos(Math.PI * tu / 180));
                    }
                    else
                    {
                        //left half
                        tx = cx - (int)(hand * -Math.Sin(Math.PI * tu / 180));
                        ty = cy - (int)(hand * Math.Cos(Math.PI * tu / 180));
                    }

                    //draw circle
                    graphics.DrawEllipse(greenPen, radarX, radarY, width, height); //bigger circle
                    graphics.DrawEllipse(greenPen, radarX + 80, radarY + 80, width - 160, height - 160); //smaller circle

                    //draw perpendicular line
                    graphics.DrawLine(greenPen, new Point(cx, radarY), new Point(cx, height)); //UP-DOWN
                    graphics.DrawLine(greenPen, new Point(radarX, cy), new Point(width, cy)); //LEFT-RIGHT

                    //draw HAND
                    graphics.DrawLine(whitePen, new Point(cx, cy), new Point(tx, ty)); //white line
                    graphics.DrawLine(greenPen, new Point(cx, cy), new Point(x, y)); //green line

                    //dispose
                    whitePen.Dispose();
                    greenPen.Dispose();
                    graphics.Dispose();

                    //update
                    u += 5;
                    if (u == 360)
                        u = 0;

                    //load radar to picturebox
                    Invoke((MethodInvoker)(() => pictureBox1.Refresh()));

                    Thread.Sleep(50);
                }catch (Exception) {}
            }
        }

        //NAV Part
        //necessary data of map
        double mapX;
        double mapY;
        int mapXHead = 60;
        int mapYHead = 90;
        int mapXStep = 86;
        int mapYStep = 116;
        int mapYTail = 60;
		
		//"Locate" button
        private void metroButton5_Click(object sender, EventArgs e)
        {
            double longitude = double.Parse(metroTextBox1.Text);
            double latitude = double.Parse(metroTextBox2.Text);
            double remainder;

            //validation of data according to map
            if (longitude < 44.5 || longitude > 54.2)
            {
                MessageBox.Show("The entered values are incorrect!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (latitude < 37.6 || latitude > 42.5)
            {
                MessageBox.Show("The entered values are incorrect!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //translating Longitude, Latitude to X & Y respectively
            remainder = longitude % 5;
            if (longitude % 10 >= 5)
                mapX = Math.Floor(mapXHead + remainder * mapXStep);
            else
                mapX = Math.Floor(mapXHead + (remainder + 5) * mapXStep);

            //быдло код, но похуй уже
            remainder = latitude % 2;
            if (latitude % 10 >= 1)
                mapY = Math.Floor(mapYHead + remainder * mapYStep);
            else
                mapY = Math.Floor(mapYHead + (remainder + 2) * mapYStep);

            //locating airplane on map
            pictureBox1.Controls.Add(pictureBox2);
            pictureBox2.Image = Properties.Resources.airplane_black;
            if (latitude / 10 < 4)
                pictureBox2.Location = new Point((int)mapX - pictureBox2.Width / 2, pictureBox1.Size.Height - (int)mapY + mapYTail - pictureBox2.Height / 2);
            else
                pictureBox2.Location = new Point((int)mapX - pictureBox2.Width / 2, (int)mapY - pictureBox2.Height / 2);
            pictureBox2.BackColor = Color.Transparent;

            metroLabel1.Focus();
        }

        //validation of pressed keys for Longitude & Latitude
        private void metroTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ','))
                e.Handled = true;
        }

        private void metroTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ','))
                e.Handled = true;
        }

        //FLYING Part
        System.Windows.Forms.Timer flyTimer = new System.Windows.Forms.Timer();
        bool flyMode; //false = Simple, true = Advanced
        List<string> COORDS = new List<string>(); //Dijkstra path coords
        int iterator = 0; //iterator for COORDS
        int subiterator = 0;
        int step = 0;
        int moveCount = 10;
        List<double> azimuthDiff = new List<double>(); // turned degrees list

        private void metroRadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            flyMode = false;
        }

        private void metroRadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            flyMode = true;
        }

        //"Flying Start" button, rotates airplane and starts timer
        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (metroRadioButton3.Checked == false && metroRadioButton4.Checked == false)
            {
                MessageBox.Show("Choose fly mode", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (flyMode)
            {
                var graph = new Graph();

                //adding vertices including plane and endpoint itselves
                string planeCoord = (pictureBox2.Location.X + pictureBox2.Size.Width / 2) + " " + (pictureBox2.Location.Y + pictureBox2.Size.Height / 2);
                graph.AddVertex(planeCoord);
                for (int i = 0; i < vertices.Length; i++)
                {
                    graph.AddVertex(vertices[i]);
                }
                string pointCoord = (pictureBox3.Location.X + pictureBox3.Size.Width / 2) + " " + (pictureBox3.Location.Y + pictureBox3.Size.Height); // (height / 2)
                graph.AddVertex(pointCoord);

                //adding edges between all vertices
                string[] data;
                data = shortestPath2vertex(planeCoord);
                graph.AddEdge(planeCoord, data[0] + " " + data[1], int.Parse(data[2]));
                for (int i = 0; i < edges.Length; i++)
                {
                    string[] values = extractGraphValues(edges[i]);
                    string firstVertex = vertices[int.Parse(values[0]) - 1];
                    string secondVertex = vertices[int.Parse(values[1]) - 1];
                    values = extractGraphValues(firstVertex);
                    int x1 = int.Parse(values[0]);
                    int y1 = int.Parse(values[1]);
                    values = extractGraphValues(secondVertex);
                    int x2 = int.Parse(values[0]);
                    int y2 = int.Parse(values[1]);
                    int weight = (int)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                    graph.AddEdge(firstVertex, secondVertex, weight);
                }
                data = shortestPath2vertex(pointCoord);
                graph.AddEdge(pointCoord, data[0] + " " + data[1], int.Parse(data[2]));

                var dijkstra = new Dijkstra(graph);
                var path = dijkstra.FindShortestPath(planeCoord, pointCoord);
                path2coords(path);

                getPoints();
            }

            Bitmap plane = new Bitmap(pictureBox2.Image);
            Bitmap rotatedPlane = RotateImage(plane, flyMode);
            pictureBox2.Image = rotatedPlane;

            flyTimer.Interval = 1000;
            flyTimer.Tick += fly_Tick;
            flyTimer.Start();

            metroLabel1.Focus();
        }

        //function moves airplane to end point, "Line drawing algorithm" is used
        private void fly_Tick(object sender, EventArgs e)
        {
            bool isReady = false; //for not to run over the point
            if (!flyMode)
            {
                int x1 = pictureBox2.Location.X;
                int y1 = pictureBox2.Location.Y;
                int x2 = pictureBox3.Location.X;
                int y2 = pictureBox3.Location.Y;
                int dx = x2 - x1;
                int dy = y2 - y1;
                int x = x1;

                //plane moving toward endpoint
                if (pictureBox2.Location.X < pictureBox3.Location.X)
                    x += 10;
                else
                    x -= 10;

                //stop flying if airplane is close to end point
                if (Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) <= 15)
                {
                    isReady = true;
                    stopFlying();
                }

                if (!isReady)
                {
                    int y;
                    y = y1 + dy * (x - x1) / dx;
                    pictureBox2.Location = new Point(x, y);
                    metroPanel2.Location = new Point(x + pictureBox2.Size.Width, y - pictureBox2.Size.Height + 20);
                    double[] data = xy2ll(x + pictureBox2.Size.Width / 2, y + pictureBox2.Size.Height / 2);
                    metroTextBox1.Text = data[0].ToString();
                    metroTextBox2.Text = data[1].ToString();
                }
            }
            if (flyMode)
            {
                string[] values;
                /*
                 * getting the last of last point of 'points'(2D Array)
                values = extractGraphValues(points[points.Count - 1][points[points.Count - 1].Count - 1]);
                int lastX = int.Parse(values[0]);
                int lastY = int.Parse(values[1]);
                listBox1.Items.Add("dx = " + dx + " | " + "dy = " + dy + " | " + "k = " + k);
                */
                if (subiterator == 0)
                {
                    values = extractGraphValues(points[iterator][0]);
                    int firstX = int.Parse(values[0]);
                    int firstY = int.Parse(values[1]);
                    values = extractGraphValues(points[iterator][points[iterator].Count - 1]);
                    int lastX = int.Parse(values[0]);
                    int lastY = int.Parse(values[1]);
                    int dx = Math.Abs(lastX - firstX);
                    int dy = Math.Abs(lastY - firstY);
                    double k = (double)dx / dy;

                    if (k < 0.5)
                        moveCount = 1;
                    else
                        moveCount = 10;

                    step = (points[iterator].Count - 1) / moveCount;
                }
                subiterator += moveCount;
                if (step == 0)
                {
                    values = extractGraphValues(points[iterator][points[iterator].Count - 1]);
                    subiterator = 0;
                    iterator++;
                    Bitmap plane = new Bitmap(pictureBox2.Image);
                    Bitmap rotatedPlane = RotateImage(plane, flyMode);
                    pictureBox2.Image = rotatedPlane;
                }
                else
                    values = extractGraphValues(points[iterator][subiterator]);
                int x = int.Parse(values[0]);
                int y = int.Parse(values[1]);
                pictureBox2.Location = new Point(x, y);
                metroPanel2.Location = new Point(x + pictureBox2.Size.Width, y - pictureBox2.Size.Height + 20);
                double[] data = xy2ll(x + pictureBox2.Size.Width / 2, y + pictureBox2.Size.Height / 2);
                metroTextBox1.Text = data[0].ToString();
                metroTextBox2.Text = data[1].ToString();
                if (iterator + 1 == COORDS.Count)
                    stopFlying();
                step--;
            }

            //calculate azimuth & distance
            if (metroRadioButton2.Checked == true)
                isPlaneInRadarSection();
        }

        //X=500, Y=260 -> Airport coordinates
        private void isPlaneInRadarSection()
        {
            if (Math.Sqrt(Math.Pow(500 - (pictureBox2.Location.X + pictureBox2.Size.Width / 2), 2) +
                    Math.Pow(260 - (pictureBox2.Location.Y + pictureBox2.Size.Height / 2), 2)) < 160)
            {
                metroPanel2.BackColor = Color.FromArgb(230, Color.Black);
                metroPanel2.Visible = true;

                //calculate azimuth
                float xDiff = 500 - (pictureBox2.Location.X + pictureBox2.Size.Width / 2);
                float yDiff = 260 - (pictureBox2.Location.Y + pictureBox2.Size.Height / 2);
                metroLabel5.Text = getAngle(yDiff, xDiff, true).ToString();

                //calculate distance
                double distance = Math.Sqrt(Math.Pow(500 - (pictureBox2.Location.X + pictureBox2.Size.Width / 2), 2) +
                                            Math.Pow(260 - (pictureBox2.Location.Y + pictureBox2.Size.Height / 2), 2));
                metroLabel6.Text = Math.Round(distance).ToString();
            }
            else
                metroPanel2.Visible = false;
        }

        //rotate airplane facing to end point
        private Bitmap RotateImage(Bitmap bmp, bool flyMode)
        {  
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                //set the rotation point to the center in the matrix
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                //calculate Y & X difference
                float xDiff, yDiff;
                if (!flyMode)
                {
                    xDiff = (pictureBox3.Location.X + pictureBox3.Size.Width / 2) - (pictureBox2.Location.X + pictureBox2.Size.Width / 2);
                    yDiff = (pictureBox3.Location.Y + pictureBox3.Size.Height / 2) - (pictureBox2.Location.Y + pictureBox2.Size.Height / 2);
                }
                else
                {
                    string[] values = { " ", " " };
                    if (iterator + 1 != COORDS.Count)
                        values = extractGraphValues(COORDS[iterator + 1]);
                    else
                        values = extractGraphValues(COORDS[iterator]);
                    int x2 = int.Parse(values[0]);
                    int y2 = int.Parse(values[1]);
                    xDiff = x2 - (pictureBox2.Location.X + pictureBox2.Size.Width / 2);
                    yDiff = y2 - (pictureBox2.Location.Y + pictureBox2.Size.Height / 2);
                }
                //rotate
                g.RotateTransform((float)getAngle(yDiff, xDiff, false));
                //restore rotation point in the matrix
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                //draw the image on the bitmap
                g.DrawImage(bmp, new Point(0, 0));
                return rotatedImage;
            }
        }

        //stop airplane motion
        private void stopFlying()
        {
            pictureBox1.Controls.Remove(pictureBox3);
            if (COORDS != null)
                COORDS.Clear();
            if (azimuthDiff != null)
                azimuthDiff.Clear();
            iterator = 0;
            subiterator = 0;

            flyTimer.Stop();
        }

        //function takes differents between Y & X of 2 targets and
        //calculates azimuth in various ways depending on radar activity
        private double getAngle(float yDiff, float xDiff, bool isRadar)
        {
            //atan2 function defines angle between 2 targets
            double azimuth = Math.Round(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
            //subsequent angle correction
            if (isRadar == true)
            {
                if (azimuth > 0)
                    azimuth = (azimuth <= 90) ? azimuth + 270 : azimuth - 90;
                else
                    azimuth = azimuth + 270;
            }
            else
            {
                if (azimuth > 0)
                    azimuth = azimuth + 90;
                else
                    azimuth = (azimuth >= -90) ? azimuth + 90 : azimuth + 450;
            }
            azimuthDiff.Add(azimuth);
            if (iterator >= 1)
                azimuth -= azimuthDiff[iterator - 1];
            return azimuth;
        }

        //"Reset" button - returns program to original state 
        private void metroButton4_Click(object sender, EventArgs e)
        {
            flyTimer.Stop();
            pictureBox1.Controls.Remove(pictureBox2);
            pictureBox1.Controls.Remove(pictureBox3);
            metroTextBox1.Clear();
            metroTextBox2.Clear();
            metroRadioButton1.Checked = true;
            metroLabel5.Text = "0";
            metroLabel6.Text = "0";
            metroPanel2.Visible = false;
            iterator = 0;
            subiterator = 0;
            if (COORDS != null)
                COORDS.Clear();
            if (azimuthDiff != null)
                azimuthDiff.Clear();
            metroLabel1.Focus();
        }

        //GRAPH Part
        //vertices initialization
        string[] vertices = new string[]
        {
          "83 208", "75 166", "168 201", "101 235", "112 274",
          "173 236", "200 139", "208 172", "281 181", "247 252",
          "266 236", "310 200", "314 253", "381 144", "396 83",
          "435 52", "450 39", "526 31", "401 224", "395 249",
          "333 363", "378 316", "416 312", "451 290", "458 244",
          "494 355", "491 429", "489 458", "484 514", "602 540",
          "532 346", "631 311", "518 305", "497 307", "627 248",
          "626 220", "620 149", "612 75", "426 109", "454 89",
          "436 179", "492 161", "545 185", "497 224", "516 233",
          "587 261", "588 290", "550 274", "500 260", "227 207"
        };

        //edges initialization
        string[] edges = new string[]
        {
            "1 2", "2 3", "3 11", "4 6", "5 6",
            "5 10", "6 9", "6 10", "6 50", "7 8",
            "8 12", "8 13", "8 50", "9 50", "10 11",
            "10 13", "11 12", "11 13", "12 13", "12 14",
            "12 19", "13 20", "13 22", "14 25", "14 39",
            "15 16", "15 39", "16 17", "16 40", "17 37",
            "18 38", "18 40", "18 42", "19 20", "19 25",
            "19 41", "20 23", "20 25", "21 22", "21 23",
            "22 25", "22 27", "23 24", "23 26", "24 25",
            "24 34", "25 35", "25 44", "25 49", "26 27",
            "26 31", "26 34", "27 28", "27 30", "28 29",
            "29 30", "30 31", "31 33", "31 47", "32 35",
            "32 47", "33 34", "33 48", "33 49", "34 49",
            "35 36", "35 46", "36 37", "36 43", "37 38",
            "38 43", "39 40", "39 42", "40 42", "41 42",
            "42 43", "42 44", "43 45", "43 46", "44 45",
            "44 49", "45 48", "45 49", "46 47", "46 48",
            "47 48", "48 49"
        };

        //get X & Y from "X Y" string
        private string[] extractGraphValues(string value2extract)
        {
            string[] values = new string[2];
            values[0] = value2extract.Split(' ').First();
            values[1] = value2extract.Split(' ').Last();
            return values;
        }

        //find coord and distance from aimPoint(plane, endPoint) to the nearest point
        private string[] shortestPath2vertex(string value2extract)
        {
            string[] minDistPoint = new string[3];
            string[] aimValues;
            aimValues = extractGraphValues(value2extract);
            string[] verticesValues;
            int min = int.MaxValue;
            int x1 = int.Parse(aimValues[0]);
            int y1 = int.Parse(aimValues[1]);
            for (int i = 0; i < vertices.Length; i++)
            {   
                verticesValues = extractGraphValues(vertices[i]);
                int x2 = int.Parse(verticesValues[0]);
                int y2 = int.Parse(verticesValues[1]);
                int weight = (int)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                if (weight < min)
                {
                    minDistPoint[0] = x2.ToString();
                    minDistPoint[1] = y2.ToString();
                    minDistPoint[2] = weight.ToString();
                    min = weight;
                }
            }
            return minDistPoint;
        }

        //separate path string to array values
        private void path2coords(string path)
        {
            string[] words = path.Split('-');
            foreach (var word in words)
                COORDS.Add(word);
            //COORDS.RemoveAt(0); //remove plane coordinate
        }

        //reset plane
        private void resetPlane()
        {
            int x = pictureBox2.Location.X;
            int y = pictureBox2.Location.Y;
            pictureBox1.Controls.Remove(pictureBox2);
            pictureBox1.Controls.Add(pictureBox2);
            pictureBox2.Location = new Point(x, y);
        }

        //get all points(line points) between all fly points
        List<List<string>> points;
        private void getPoints()
        {
            points = new List<List<string>>();
            for (int i = 1; i < COORDS.Count; i++)
            {
                string[] values = extractGraphValues(COORDS[i - 1]);
                int x1 = int.Parse(values[0]);
                int y1 = int.Parse(values[1]);
                values = extractGraphValues(COORDS[i]);
                int x2 = int.Parse(values[0]);
                int y2 = int.Parse(values[1]);
                int dx = x2 - x1;
                int dy = y2 - y1;
                List<string> subPoint = new List<string>();
                if (x1 < x2)
                {
                    for (int x = x1; x < x2; x++)
                    {
                        y = y1 + dy * (x - x1) / dx;
                        subPoint.Add((x - pictureBox2.Size.Width / 2) + " " + (y - pictureBox2.Size.Height / 2));
                    }
                }
                else
                {
                    for (int x = x1; x > x2; x--)
                    {
                        y = y1 + dy * (x - x1) / dx;
                        subPoint.Add((x - pictureBox2.Size.Width / 2) + " " + (y - pictureBox2.Size.Height / 2));
                    }
                }
                points.Add(subPoint);
            }
        }

        //translating X, Y to Longitude & Latitude respectively
        private double[] xy2ll(int x, int y)
        {
            double[] data = new double[2];

            if (x <= mapXHead)
            {
                if (x == mapXHead)
                    data[0] = 45;
                else
                    data[0] = 44 + Math.Round((double)x / mapXHead, 1);
            }
            else
            {
                x -= mapXHead;
                double step = Math.Round((double)x / mapXStep, 1);
                data[0] = 45 + step;
            }

            if (y <= mapYHead)
            {
                if (y == mapYHead)
                    data[1] = 42;
                else
                    data[1] = 43 - Math.Round((double)y / mapYHead, 1);
            }
            else
            {
                y -= mapYHead;
                double step = Math.Round((double)y / mapYStep, 1);
                data[1] = 42 - step;
            }

            return data;
        }

        //all below saved for future tests
        /*private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            MessageBox.Show(mouse.X + " " + mouse.Y);
        }
        
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            foreach (var subPoint in points)
                foreach (var value in subPoint)
                    listBox1.Items.Add(value);
            listBox1.Items.Add(points[0][3]);
        }

        //List<string> points = new List<string>();
        //Bresenham's line-algorithm [all cases]
        public void BresenhamLine(int x, int y, int x2, int y2)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                //points.Add(x + " " + y);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }*/
    }
}