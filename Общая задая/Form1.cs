using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Общая_задая
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //textBox7.ScrollBars = ScrollBars.Vertical;
        }
        static int n , T, layers;
        static double B, M1, M2, E, deltat, Alfa, I0, E1,E2, Bet, R1, R2, q;
        DataTable table;






        private void button4_Click(object sender, EventArgs e)
        {
            /* Новое:*/

            double[,,] u = new double[layers, n, n]; // Текущий
            double[,,] u1 = new double[layers, n, n]; // Текущий 

            double[] w = new double[n]; // Омега 

            /*-------------------*/
            // Добавлено новое:
            deltat = (double)T / q;
            double[,,] weigth = new double[layers, n, n]; // Текущий   
            double[,,] weigth1 = new double[layers, n, n]; // предыдущий
            // Массив X
            double[,] x = new double[layers + 1, n];
            double[,] x1 = new double[layers + 1, n];
            double[] A = new double[n]; // массив Ai
            double[] gamma = new double[n];


            double[,] p = new double[layers + 1, n];
            double[,] p1 = new double[layers + 1, n];

            InitAi(ref A);
            IntitWeigth(ref weigth);

            double I, I1;

            InitX(ref x);
            InitGamma(ref gamma);
            CalcNewValX(ref weigth, ref x, ref gamma);
            I = NextTgtFuncVal(ref weigth, ref x, ref A); // возвращает I
            I0 = I;
            I1 = I;
 
            Iteration(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma, ref I1);
               
            FileInput(ref x, @"MyTestX.txt");
            FileInput(ref p, @"MyTestP.txt");
            FileInitArrWeigth(ref weigth);
            MessageBox.Show("The end is now!");
            


           
        }


        void FileInput (ref double[,] x, string path)
        {
            //File.Create("new_file.txt");
            string ch = "";
            //if (!File.Exists(path))
            //{
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    for (int i = 0; i < n; i++) { 
                        for (int k = 0; k < q + 1; k++)
                         {
                            if (k == 0)
                            {
                                ch = Convert.ToString(x[k, i]);
                            }
                            else
                            {
                                ch =" "+ Convert.ToString(x[k, i]);
                                if (k == q)
                                {
                                    ch = ch+"\n";
                                }
                            }
                        
                        sw.Write(ch);
                            
                         }
                    }
                }
           // }

        }


        private void FileOutput(ref double[,] x, string path)
        {
             // потом можно удалить. Сейчас только для теста
            int k = 0;

            // Open the file to read from.
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    InitFileFromFile(ref x, k, s);
                    k++;
                }
            }



        }

        private void InitFileFromFile(ref double[,] x, int k,  String s)
        {
            string ch; // само число
            for (int i = 0; i < q; i++)
            {
                int indexStart = 0;
                int indexEnd=s.IndexOf(" ");
                ch = s.Substring(indexStart, indexEnd);
                x[i, k] = Convert.ToDouble(ch);
                s = s.Remove(0, indexEnd+1);
            }


        }













 ///Перед итерацией устанавливаем значения на предыдущей итерации
//Будет вызываться перед началом очередной итерации(кроме нулевой)
        private void SetLastValX(ref double[,] x, ref double[,] x1) // x1- предыдущее хранит
        {
            for (int k = 0; k < q + 1; k++)  /// ТУТ ПОМЕНЯЛ МЕСТАМИ
                for (int i = 0; i < n; i++)
                    x1[k, i] = x[k, i];

        }

        private void ImproveWeigths(ref double[,,] weigth, ref double[,,] weigth1, ref double[,] p, ref double[,] x1)
        {
            for (int k = 0; k < q; k++)   // ТУТ ПОМЕНЯЛ МЕСТАМИ
                for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    {
                        weigth[k, i, j] = weigth1[k, i, j];
                        weigth[k, i, j] -= Alfa * (2 * M1 * deltat * weigth1[k, i, j] - deltat * p[k + 1, i] * x1[k, j]);
                        if (weigth[k, i, j] > B)
                            weigth[k, i, j] = B;
                        else if (weigth[k, i, j] < (-1) * B)
                            weigth[k, i, j] = (-1) * B;

                    }
        }

        private void SetLastValWeigth(ref double[,,] weigth, ref double[,,] weigth1) // x1- предыдущее хранит
        {
            for (int k = 0; k < q; k++)
                for (int i = 0; i < n; i++)
                    for(int j=0; j<n; j++)
                        weigth1[k, i, j] = weigth[k, i, j];

        }
        

        private void CalcP(ref double [,]p, ref double [,]x1, ref double [,,] weigth1, ref double []A, ref double []gamma)
        {
            double sum=0;
            for (int i = 0; i < n; i++)
                p[layers, i] = -2 * M2 * (x1[layers, i] - A[i]);
          
            for (int k = layers - 1; k >= 1; k--)
                for (int i = 0; i < n; i++) { 
                    p[k, i] = p[k + 1, i] - deltat * gamma[i] * p[k + 1, i];
                    sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sum += p[k + 1, j] * weigth1[k, j, i];
                    }
                    p[k, i] += deltat * sum;
                }
                  
        }

        private void InitGamma(ref double [] gamma)
        {
            // задание Г
            for (int i = 0; i < n; i++)
            {

                try
                {
                    //MessageBox.Show(Convert.ToString(dataGridView1.Rows[2].Cells[i + 1].Value));
                    gamma[i] = Convert.ToDouble(dataGridView1.Rows[2].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите Гi в таблицу");
                    return;

                }

            }
        }
        // Считывание с грида Х
        private void InitX(ref double [,]x)
        {

                for (int i = 0; i < n; i++)
                {
                    try
                    {
                    // было dataGridView1.CurrentRow.Cells[i + 1].Value
                    
                        x[0, i] = Convert.ToDouble(dataGridView1.Rows[0].Cells[i + 1].Value);  
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Введите ai в таблицу");
                        return;
                    }

                }
            
        }


        // задаем веса W
        private void IntitWeigth(ref double [,,]weigth)
        {
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        weigth[k, i, j] = 1;
                    }
                }
            }
        }

        private void InitAi(ref double [] A)
        {
            // задание Ai
            for (int i = 0; i < n; i++)
            {

                try
                {
                    //MessageBox.Show(Convert.ToString(dataGridView1.Rows[1].Cells[i + 1].Value));
                    A[i] = Convert.ToInt32(dataGridView1.Rows[1].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите ai в таблицу");
                    return;

                }

            }
        }



        private void CalcNewValX(ref double[,,] weigth, ref double[,] x, ref double[] gamma) // вычисления на 0 итерации
        {
            // k - слой
            // i, j - нейроны
            // второй тест

            ///  ИСПРАВЛЯЛ ТУТ:

            // слагаемое первое и слагаемое второе
            for (int k = 0; k < q; k++)   
            {
                double sl2 = 0;
                for (int i = 0; i < n; i++)
                {
                    sl2 = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sl2 = sl2 + weigth[k, i, j] * x[k, j];
                    }

                    x[k + 1, i] = x[k, i] + deltat * (-gamma[i] * x[k, i] + sl2);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Считываем с формы требуемые значения

            double Xmin = 1;

            double Xmax = q;

            double Step = 1;

            double[] Ox = new double[layers];
            for (int i = 0; i < q; i++)
                Ox[i] = i + 1;

            //Значения иксов на последней итерации
            double[,] x = new double[layers + 1, n];
            FileOutput(ref x, @"MyTestX.txt");

            // Количество точек графика

            int count = (int)Math.Ceiling((Xmax - Xmin) / Step) + 1;



            // Настраиваем оси графика

            chart1.ChartAreas[0].AxisX.Minimum = Xmin;

            chart1.ChartAreas[0].AxisX.Maximum = Xmax;

            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            // Определяем шаг сетки

            chart1.ChartAreas[0].AxisX.MajorGrid.Interval = Step;

            //Очищаем добавленные "графики"
            chart1.Series.Clear();

            //Добавим серии в график
            for (int i = 0; i < n; i++)
            {

                System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
                series.ChartArea = "ChartArea1";
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series.Legend = "Legend1";
                series.Name = "X" + Convert.ToString(i);
                this.chart1.Series.Add(series);
            }

            // Добавляем вычисленные значения в графики

            for (int i = 0; i < n; i++)
            {
                double[] tempX = new double[layers];
                for (int j = 0; j < q; j++)
                    tempX[j] = x[j, i];
                chart1.Series[i].Points.DataBindXY(Ox, tempX);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Считываем с формы требуемые значения

            double Pmin = 1;

            double Pmax = q;

            double Step = 1;

            double[] Ox = new double[layers];
            for (int i = 0; i < q; i++)
                Ox[i] = i + 1;

            //Значения иксов на последней итерации
            double[,] p = new double[layers + 1, n];
            FileOutput(ref p, @"MyTestP.txt");

            // Количество точек графика

            int count = (int)Math.Ceiling((Pmax - Pmin) / Step) + 1;



            // Настраиваем оси графика

            chart2.ChartAreas[0].AxisX.Minimum = Pmin;

            chart2.ChartAreas[0].AxisX.Maximum = Pmax;

            chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            // Определяем шаг сетки

            chart2.ChartAreas[0].AxisX.MajorGrid.Interval = Step;

            //Очищаем добавленные "графики"
            chart2.Series.Clear();

            //Добавим серии в график
            for (int i = 0; i < n; i++)
            {

                System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
                series.ChartArea = "ChartArea2";
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series.Legend = "Legend2";
                series.Name = "P" + Convert.ToString(i);
                this.chart2.Series.Add(series);
            }

            // Добавляем вычисленные значения в графики

            for (int i = 0; i < n; i++)
            {
                double[] tempP = new double[layers];
                for (int j = 0; j < q; j++)
                    tempP[j] = p[j, i];
                chart2.Series[i].Points.DataBindXY(Ox, tempP);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            double[,,] weigth = new double[layers, n, n]; // Текущий
            FileOutputArrWeigth(ref weigth);

            DataTable table = new DataTable();



            for (int i = 0; i < n; i++)
            {
                table.Columns.Add("Wki" + Convert.ToString(i), typeof(double)); // задали шапку 
            }

            for (int k = 0; k < q*(n+1); k++) table.Rows.Add(); // добавляем нужное количество строчек

            dataGridView4.DataSource = table;

            for (int k = 0; k < q; k++)
            {
                dataGridView4.Rows[k*(n+1)].Cells[0].Value = k;
                for (int i =0; i<n; i++)
                {
                    for (int j = 0; j <= n - 1; j++)
                    {
                        dataGridView4.Rows[k*(n+1) + i + 1].Cells[j].Value = weigth[k, i, j];
                    }
                }
                


            }
            dataGridView4.AutoResizeColumns();
        }

        void FileInitArrWeigth(ref double[,,] weigth)
        {
            string path = @"ArrWeigth.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                for(int k=0; k<q; k++)
                {
                    sw.WriteLine(k);
                    for (int i=0; i<n; i++)
                    {
                        for(int j=0; j<n; j++)
                        {
                            sw.Write(weigth[k, i, j]+ " ");
                        }
                        sw.Write("\n");
                    }
                }


                    
                
            }
        }

        void FileOutputArrWeigth(ref double [,,] weigth)
        {
            
            string path = @"ArrWeigth.txt";
            using (StreamReader sr = File.OpenText(path))
            {
                int k; string ch;
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    k = Convert.ToInt32(s);
                    
                    for(int i=0; i<n; i++)
                    {
                        s = sr.ReadLine();
                        for (int j=0; j<n; j++)
                        {
                            int indexStart = 0;
                            int indexEnd = s.IndexOf(" ");
                            ch = s.Substring(indexStart, indexEnd);
                            weigth[k, i, j] = Convert.ToDouble(ch);
                            s = s.Remove(0, indexEnd + 1);
                            
                        }
                    }

                    
                }
            }

        }


        private double NextTgtFuncVal(ref double[,,] weigth, ref double [,] x,ref double[]A   )
        {
            double I=0, sl1=0, sl2=0;
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        sl1 += (weigth[k, i, j]) * (weigth[k, i, j]);
                    }
                }
            }
                for(int i=0; i < n; i++)
                {
                    sl2 += (x[layers, i] - A[i]) * (x[layers, i] - A[i]);
                }
                I = M1 * deltat * sl1 + M2 * sl2;
            return I;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            double[,] x = new double[layers + 1, n];
            FileOutput(ref x, @"MyTestX.txt");

            DataTable table = new DataTable();



            for (int i = 0; i < n; i++)
            {
                table.Columns.Add("X" + Convert.ToString(i), typeof(double)); // задали шапку 
            }

            for (int k = 0; k < q; k++) table.Rows.Add(); // добавляем нужное количество строчек

            dataGridView3.DataSource = table;
           
            for (int k = 0; k < q; k++)
            {

                for (int j = 0; j <= n - 1; j++)
                {
                    dataGridView3.Rows[k].Cells[j].Value=x[k,j];
                }
               

            }
            dataGridView3.AutoResizeColumns();

        }

        private double Step4_7(ref double[,] p, ref double[,] x, ref double[,] x1,
            ref double[,,] weigth, ref double[,,] weigth1, 
            ref double[] A, ref double[] gamma)
        {
            //4
            CalcP(ref p, ref x1, ref weigth1, ref A, ref gamma);

            //5
            ImproveWeigths(ref weigth, ref weigth1, ref p, ref x1);

            //6
            CalcNewValX(ref weigth, ref x, ref gamma);

            //7
            return NextTgtFuncVal(ref weigth, ref x, ref A);
        }

        
        private double Step5_7(ref double[,] p, ref double[,] x, ref double[,] x1,
    ref double[,,] weigth, ref double[,,] weigth1,
    ref double[] A, ref double[] gamma)
        {
            //5
            ImproveWeigths(ref weigth, ref weigth1, ref p, ref x1);

            //6
            CalcNewValX(ref weigth, ref x, ref gamma);

            //7
            return NextTgtFuncVal(ref weigth, ref x, ref A);
        }


        private void Iteration(ref double[,] p, ref double[,] x, ref double[,] x1,
        ref double[,,] weigth, ref double[,,] weigth1,
        ref double[] A, ref double[] gamma, ref double I1)
        {
            DataTable table = new DataTable();
            table.Columns.Add("I", typeof(double)); // задали шапку 
            List<double> listI = new List<double>();
            listI.Add(I0);
            
            SetLastValX(ref x, ref x1);
            SetLastValWeigth(ref weigth, ref weigth1);
            double tempI = Step4_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
            listI.Add(tempI);
            double ValILoop = I1;

            
            while (!(Math.Abs(tempI - I1) < E))
            {
                if (tempI > I1)
                {
                    Alfa /= 2;
                    if (Math.Abs(tempI - ValILoop) < (double)E / 10)
                    {
                        MessageBox.Show("Cannot reach");
                        break;
                    }
                    ValILoop = tempI;
                    tempI = Step5_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
                    listI.Add(tempI);
                }
                else
                {
                    //printI(Convert.ToString(tempI)); 
                    //MessageBox.Show(Convert.ToString(tempI));
                    I1 = tempI;
                    SetLastValX(ref x, ref x1);
                    SetLastValWeigth(ref weigth, ref weigth1);
                    tempI = Step4_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
                    listI.Add(tempI);
                }
            }

            foreach (var element in listI)
            {
                table.Rows.Add(); // добавляем необходимое количество строчек
            }
            dataGridView2.DataSource = table;
            int k = 0; // элемент по счету
            foreach (var element in listI)
            {
                dataGridView2.Rows[k].Cells[0].Value = Convert.ToString(element); // добавляем необходимое количество строчек
                k++;
            }

            // textBox7.Text += (Convert.ToString(tempI)) + ';' + "\n";
        }

        private void printI(string i)
        {
            //textBox7.Text += (i) + ';' + "\n";

        }


        private void button1_Click(object sender, EventArgs e)
        {
            // добавил Delt - дель
            try
            {
                n = Convert.ToInt32(textBox1.Text);
                q = Convert.ToInt32(textBox6.Text);
                T = Convert.ToInt32(textBox2.Text);
                E = Convert.ToDouble(textBox9.Text);
                Alfa = Convert.ToDouble(textBox8.Text);
                E1= Convert.ToDouble(textBox10.Text);
                E2 = Convert.ToDouble(textBox7.Text);
                Bet = Convert.ToDouble(textBox3.Text);
                R1 = Convert.ToDouble(textBox4.Text);
                R2 = Convert.ToDouble(textBox5.Text);
                layers= Convert.ToInt32(textBox11.Text);
                // Старое удалить :
                B = Convert.ToDouble(textBox3.Text);
                M1 = Convert.ToDouble(textBox4.Text);
                M2 = Convert.ToDouble(textBox5.Text);
                //
            }
            catch (Exception exc)
            {
                MessageBox.Show("Введите данные заново!");
                return;

            }
            //n = 2; q = 10; T = 1; B = 2; M1 = 2; M2 = 3;Alfa = 0.01; E = 0.001;


            // то
            DataTable table = new DataTable();

            table.Columns.Add(" ", typeof(string));

            for (int i = 0; i < n; i++)
            {
                table.Columns.Add(Convert.ToString(i + 1), typeof(double));
                
            }


            table.Rows.Add("Xi(0)");
            table.Rows.Add("Xi(T)");
            table.Rows.Add("Ai");
            table.Rows.Add("wi");
            dataGridView1.DataSource = table;
            //this.dataGridView1.Columns["1"].DefaultCellStyle.Format = "g";
            //dataGridView1.Columns('1').DefaultCellStyle.Format = "N2";
            //dataGridView1.Columns["2"].DefaultCellStyle.Format = "N2";
            dataGridView1.AutoResizeColumns();


            // то
            DataTable table1 = new DataTable();

            table1.Columns.Add(" ", typeof(string));

            for (int i = 0; i < n; i++)
            {
                table1.Columns.Add(Convert.ToString(i + 1), typeof(double));

            }


            table1.Rows.Add("U^k(0)");

            StartControlGrid.DataSource = table1;
            StartControlGrid.AutoResizeColumns();

        }




        private void button2_Click(object sender, EventArgs e)
        {
            int q, n;
            n = Convert.ToInt32(textBox1.Text);
            q = Convert.ToInt32(textBox6.Text);


            DataTable table1 = new DataTable();

            table1.Columns.Add("W", typeof(string));

            for (int i = 0; i < n; i++)
            {
                table1.Columns.Add(Convert.ToString(i + 1), typeof(int));
                table1.Rows.Add(Convert.ToString(i + 1));
            }

            dataGridView2.DataSource = table;
            dataGridView2.AutoResizeColumns();
        }
    }
}
