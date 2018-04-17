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
        //static int n, T, layers;
        //static double E, deltat, Alfa, I0, E1, E2, Bet, R1, R2, q;
        //DataTable table;

        static int n, T, q;
        static double E, deltat, Alfa, I0, E1, E2, Bet, R1, R2;
        DataTable table;

        double[] u; // Текущий
        double[] u1; // Текущий 

        double[] w; // Омега 
                    // Массив X
        double[,] x;
        double[,] x1;

        double[,] y;
        double[,] y1;

        double[] A; // массив Ai - желаемые

        double[,] g;
        double[,] g1;

        double[,] p;
        double[,] p1;





        private void button4_Click(object sender, EventArgs e) // Для инициализации
        {
            /* Новое:*/

            u = new double[q]; // Текущий
            u1 = new double[q]; // 

            w = new double[n]; // Омега 

            /*-------------------*/
            // Добавлено новое:
            deltat = (double)T / q;
            // Массив X
            x = new double[q + 1, n];
            x1 = new double[q + 1, n];

            y = new double[q + 1, n];
            y1 = new double[q + 1, n];

            A = new double[n]; // массив Ai

            g = new double[q + 1,  n];
            g1 = new double[q + 1, n];

            p = new double[q + 1, n];
            p1 = new double[q + 1, n];

           

            InitAi(ref A);
            InitControls(ref u); // инициализация u
            InitY(ref y);
            double I, I1;

            InitX(ref x); // +
            InitOmega(ref w); //

            CalcNewValXAndNewValY(ref x, ref y, ref u, ref w);
            
            I = NextTgtFuncVal(ref x, ref A, ref u); // возвращает I
            I0 = I;
            I1 = I;
 
    //        Iteration(ref x, ref x1, ref u, ref u1, ref y, ref y1, ref p, ref g, ref A, ref w, ref I1);
               
            FileInput(ref x, @"MyTestX.txt");
            FileInput(ref p, @"MyTestP.txt");
            //FileInitArrWeigth(ref weigth);
            MessageBox.Show("The end is now!");
            


           
        }

        private void CalcNewValXAndNewValY(ref double[,] x_, ref double[,] y_, ref double[] u_, ref double[] w_)
        {
            // k - слой
            // i, j - нейроны
            // второй тест
            double sl3 = 0;
                        
            // слагаемое первое и слагаемое второе
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    x_[k + 1, i] = x_[k, i] + deltat* y_[k, i];

                    for(int j=0; j<n; j++)
                    {
                        sl3 = y_[k, j] - y_[k, i];
                    }
                    sl3 = sl3* E2 * u_[k] / n;
                    y_[k + 1, i] = y_[k, i] + deltat * (-(w_[i] * w_[i]) * x_[k, i] + E1 * (1 - Bet * (x_[k, i] * x_[k, i]))+sl3);
                }
            }
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



        private void InitY(ref double[,] y_)
        {
            for (int i = 0; i < n; i++)
            {
                y_[0, i] = 1;
            }
        }









        ///Перед итерацией устанавливаем значения на предыдущей итерации
        //Будет вызываться перед началом очередной итерации(кроме нулевой)
        private void SetLastValXAndValY(ref double[,] x_, ref double[,] x1_, ref double[,] y_, ref double[,] y1_) // x1- предыдущее хранит
        {
            for (int k = 0; k < q + 1; k++)  /// ТУТ ПОМЕНЯЛ МЕСТАМИ
                for (int i = 0; i < n; i++)
                {
                    x1_[k, i] = x_[k, i];
                    y1_[k, i] = y[k, i];
                }

        }
        // Корректировка управления 
        private void ImproveU(ref double[] u_, ref double[] u1_, ref double[,] g_,  ref double[,] y_ ) // Проверить то ли мы передаем
        {
            double sl1=0, sl2=0, sl3=0;
            for (int k = 0; k < q; k++)
            {   // ТУТ ПОМЕНЯЛ МЕСТАМИ
                /// Дописать условие, по которому будет выполняться проверка, 
                /// нужно ли сохранять предыдущее значение
                u1_[k] = u_[k];  
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        sl3 += y_[k, j] - y_[k, i];
                    }
                    sl2 += sl3 * g_[k + 1, i];

                }
                sl2 *= E2 / n;
                sl1 = R1 * deltat * u_[k] + sl2;
                u_[k] = u1_[k] - Alfa *sl1;
            }
        }

        private void SetLastValWeigth(ref double[,,] weigth, ref double[,,] weigth1) // x1- предыдущее хранит
        {
            for (int k = 0; k < q; k++)
                for (int i = 0; i < n; i++)
                    for(int j=0; j<n; j++)
                        weigth1[k, i, j] = weigth[k, i, j];

        }
        

        //private void CalcP(ref double [,]p, ref double [,]x1, ref double [,,] weigth1, ref double []A, ref double []gamma)
        //{
        //    double sum=0;
        //    for (int i = 0; i < n; i++)
        //        p[q, i] = -2 * M2 * (x1[q, i] - A[i]);
          
        //    for (int k = q - 1; k >= 1; k--)
        //        for (int i = 0; i < n; i++) { 
        //            p[k, i] = p[k + 1, i] - deltat * gamma[i] * p[k + 1, i];
        //            sum = 0;
        //            for (int j = 0; j < n; j++)
        //            {
        //                sum += p[k + 1, j] * weigth1[k, j, i];
        //            }
        //            p[k, i] += deltat * sum;
        //        }
                  
        //}

        private void CalcPAndG( ref double[,] x_, ref double[] u_, ref double [,] p_, ref double[,] g_, ref double[] A_, ref double[] w_)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                p_[q, i] = -2 * R2 * (x_[q, i] - A_[i]);   /// Нужно поменять по _
                g_[q, i] = 0;
            }
                
            for (int k = q-1; k >= 1; k--)  /// ТТТТТТТТТТТТТТУУУУТ ПРАВИЛ c q-1 на q
                for (int i = 0; i < n; i++)
                {
                    p_[k, i] = p_[k + 1, i] - g_[k + 1, i] * deltat * (w_[i] * w_[i] + 2 * E1 * Bet * x_[k, i]); 
                    sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sum += g_[k + 1, j];
                    }
                    sum = 1 / n * sum;
                    g_[k, i] = deltat * p_[k + 1, i] + g_[k + 1, i] - deltat * E2 * u_[k] * (g_[k + 1, i] - sum);
                }
        }

        private void InitAi(ref double[] A_)
        {
            // задание Ai
            for (int i = 0; i < n; i++)
            {

                try
                {
                    //MessageBox.Show(Convert.ToString(dataGridView1.Rows[1].Cells[i + 1].Value));
                    A_[i] = Convert.ToInt32(dataGridView1.Rows[1].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите ai в таблицу");
                    return;

                }

            }
        }
        // задаем веса U
        private void InitControls(ref double[] u_)
        {
            for (int i = 0; i < n; i++)
            {

                try
                {
                    //MessageBox.Show(Convert.ToString(dataGridView1.Rows[1].Cells[i + 1].Value));
                    u_[i] = Convert.ToInt32(StartControlGrid.Rows[0].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите Ui в таблицу");
                    return;

                }
            }
        }
        // Считывание с грида Х
        private void InitX(ref double[,] x_)
        {

            for (int i = 0; i < n; i++)
            {
                try
                {
                    // было dataGridView1.CurrentRow.Cells[i + 1].Value

                    x_[0, i] = Convert.ToDouble(dataGridView1.Rows[0].Cells[i + 1].Value);
                    //x_[q - 1, i] = Convert.ToDouble(dataGridView1.Rows[1].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите Xi в таблицу");
                    return;
                }

            }

        }
        private void InitOmega(ref double[] w_)
        {
            // задание Г
            for (int i = 0; i < n; i++)
            {

                try
                {
                    //MessageBox.Show(Convert.ToString(dataGridView1.Rows[2].Cells[i + 1].Value));
                    w_[i] = Convert.ToDouble(dataGridView1.Rows[2].Cells[i + 1].Value);
                }
                catch (Exception)
                {
                    MessageBox.Show("Введите Wi в таблицу");
                    return;

                }

            }
        }





        //private void CalcNewValY(ref double[,] y_, ref double[,] x, ref double[,,] u, ref double[] w) // вычисления на 0 итерации
        //{
        //    // k - слой
        //    // i, j - нейроны
        //    // второй тест

        //    ///  ИСПРАВЛЯЛ ТУТ:

        //    // слагаемое первое и слагаемое второе
        //    for (int k = 0; k < q; k++)
        //    {
        //        double sl2 = 0;
        //        for (int i = 0; i < n; i++)
        //        {
        //            sl2 = 0;
        //            for (int j = 0; j < n; j++)
        //            {
        //                sl2 = sl2 + weigth[k, i, j] * x[k, j];
        //            }

        //            x[k + 1, i] = x[k, i] + deltat * (-gamma[i] * x[k, i] + sl2);
        //        }
        //    }
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            // Считываем с формы требуемые значения

            double Xmin = 1;

            double Xmax = q;

            double Step = 1;

            double[] Ox = new double[q];
            for (int i = 0; i < q; i++)
                Ox[i] = i + 1;

            //Значения иксов на последней итерации
            double[,] x = new double[q + 1, n];
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
                double[] tempX = new double[q];
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

            double[] Ox = new double[q];
            for (int i = 0; i < q; i++)
                Ox[i] = i + 1;

            //Значения иксов на последней итерации
            double[,] p = new double[q + 1, n];
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
                double[] tempP = new double[q];
                for (int j = 0; j < q; j++)
                    tempP[j] = p[j, i];
                chart2.Series[i].Points.DataBindXY(Ox, tempP);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            double[,,] weigth = new double[q, n, n]; // Текущий
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


        private double NextTgtFuncVal( ref double [,] x_,ref double[]A_, ref double[] u_)
        {
            ///Условие, при котором нужно сохранять предыдущее значение функции

            double I=0, sl1=0, sl2=0;
            for (int k = 0; k < q; k++)
            {
                sl1+= u_[k] * u_[k] / 2;
            }
            sl1 *= R1 * deltat;
            for (int i=0; i < n; i++)
            {
             sl2 += (x_[q, i] - A_[i])* (x_[q, i] - A_[i]);
            }
            sl2 *= R2;
                I = sl1+sl2;
            return I;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            double[,] x = new double[q + 1, n];
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

        private double Step4_7(ref double[,] x_, ref double[,] x1_,
            ref double[] u_, ref double[] u1_,
            ref double[,] y_,
            ref double[,] p_, ref double [,] g_,
            ref double[] A_, ref double[] w_)
        {
            //4
            CalcPAndG(ref x_, ref u_, ref p_, ref g_, ref A_,ref w_); // Add

            //5
            ImproveU(ref u_, ref u1_, ref g_, ref y_);

            //6
            CalcNewValXAndNewValY(ref x_, ref y_, ref u_, ref w_);

            //7
            return NextTgtFuncVal(ref x_, ref A_, ref u_);
        }

        
        private double Step5_7(ref double[,] x_,
            ref double[] u_, ref double[] u1_,
            ref double[,] y_,
            ref double[,] p_, ref double[,] g_,
            ref double[] A_, ref double[] w_)
        {
            //4
            CalcPAndG(ref x_, ref u_, ref p_, ref g_, ref A_, ref w_); // Add

            //5
            ImproveU(ref u_, ref u1_, ref g_, ref y_);

            //7
            return NextTgtFuncVal(ref x_, ref A_, ref u_);
        }


        private void Iteration(ref double[,] x_, ref double[,] x1_, ref double[] u_, ref double[] u1_,
            ref double[,] y_, ref double[,] y1_, ref double[,] p_, ref double[,] g_,
            ref double[] A_, ref double[] w_, ref double I1)
        {
            DataTable table = new DataTable();
            table.Columns.Add("I", typeof(double)); // задали шапку 
            List<double> listI = new List<double>();
            listI.Add(I0);
            
            //SetLastValXAndValY(ref x, ref x1, ref y, ref y1);
            SetlastU(ref u,ref u1);
            double tempI = Step4_7(ref x_, ref x1_, ref u_, ref u1_, ref y_, ref p_, ref g_, ref A_, ref w_);
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
                    tempI = Step5_7(ref x_, ref u_, ref u1_, ref y_, ref p_, ref g_, ref A_, ref w_);
                    listI.Add(tempI);
                }
                else
                {
                    //printI(Convert.ToString(tempI)); 
                    //MessageBox.Show(Convert.ToString(tempI));
                    I1 = tempI;
                    //SetLastValXAndValY(ref x_, ref x1_, ref y_, ref y1_);
                    SetlastU(ref u_, ref u1_);
                    tempI = Step4_7(ref x_, ref x1_, ref u_, ref u1_, ref y_, ref p_, ref g_, ref A_, ref w_);
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

        private void SetlastU(ref double[] u_, ref double[] u1_)
        {
            for(int  i =0; i<q; i++)
            {
                u1[i] = u[i];
            }
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
            //table.Rows.Add("Xi(T)");
            table.Rows.Add("Ai");
            table.Rows.Add("wi");
            dataGridView1.DataSource = table;
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
