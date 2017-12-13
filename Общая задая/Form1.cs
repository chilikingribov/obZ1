﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Общая_задая
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static int n = 1, T,  q = 1;
        static double B, M1, M2, E, deltat, Alfa;
        DataTable table;
        




       
        private void button4_Click(object sender, EventArgs e)
        {

      

            // Добавлено новое:
            deltat = (double)T / q;
            double[,,] weigth = new double[q, n, n]; // Текущий
            double[,,] weigth1 = new double[q, n, n]; ;// предыдущий
            // Массив X
            double[,] x = new double[q+1, n];
            double[,] x1 = new double[q+1, n];
            double[] A = new double[n]; // массив Ai
            double[] gamma = new double[n];


            double[,] p = new double[q+1, n];
            double[,] p1 = new double[q+1, n];





            InitAi(ref A);
            IntitWeigth(ref weigth);






            double I, I1;

            InitX(ref x);
            InitGamma(ref gamma);
            //Iter0(ref weigth, ref x, ref gamma);
            CalcNewValX(ref weigth, ref x, ref gamma);
            I = NextTgtFuncVal(ref weigth, ref x, ref A); // возвращает I
            I1 = I;
            textBox7.Text = Convert.ToString(I);
            // вставить инициализацию предыдущих значений
            //
            Iteration(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma, ref I1);
            MessageBox.Show("The end is now!");
        }
 ///Перед итерацией устанавливаем значения на предыдущей итерации
//Будет вызываться перед началом очередной итерации(кроме нулевой)
        private void SetLastValX(ref double[,] x, ref double[,] x1) // x1- предыдущее хранит
        {
            for (int i = 0; i < n; i++)
                for (int k = 0; k < q + 1; k++)
                    x1[k, i] = x[k, i];

        }

        private void ImproveWeigths(ref double[,,] weigth, ref double[,,] weigth1, ref double[,] p, ref double[,] x1)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < q; k++)
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
            {
                p[q, i] = -2 * M2 * (x1[q, i] - A[i]);
          
                for (int k = q - 1; k < 1; k--){
                    p[k, i] = p[k + 1, i] - deltat * gamma[i] * p[k + 1, i];

                    for (int j = 0; j < n; j++)
                    {
                        sum += p[k + 1, j] * weigth1[k, i, j];
                    }
                    p[k, i] += deltat * sum;
                }
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
                        x[0, i] = Convert.ToInt32(dataGridView1.CurrentRow.Cells[i + 1].Value);  
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


        //private void Iter0(ref double [,,] weigth, ref double [,] x, ref double []gamma) // вычисления на 0 итерации
        //{
        //    // k - слой
        //    // i, j - нейроны
        //    // второй тест


        //        // слагаемое первое и слагаемое второе
        //        for (int i = 0; i < n; i++)
        //        {
        //        double sl2 = 0;
        //        for (int k = 0; k < q - 1; k++)
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

        private void CalcNewValX(ref double[,,] weigth, ref double[,] x, ref double[] gamma) // вычисления на 0 итерации
        {
            // k - слой
            // i, j - нейроны
            // второй тест


            // слагаемое первое и слагаемое второе
            for (int i = 0; i < n; i++)
            {
                double sl2 = 0;
                for (int k = 0; k < q; k++)
                {
                    sl2 = 0;
                    for (int j = 0; j < n; j++)
                    {
                        sl2 = sl2 + weigth[k, i, j] * x[k + 1, j];
                    }

                    x[k + 1, i] = x[k, i] + deltat * (-gamma[i] * x[k, i] + sl2);
                }
            }
        }

        private double NextTgtFuncVal(ref double[,,] weigth, ref double [,] x,ref double[]A   )
        {
            double I=0, sl1=0, sl2=0;
            for(int k=0; k<q; k++)
            {
                for(int i=0; i < n; i++)
                {
                    for(int j=0; j<n; j++)
                    {
                        sl1 += (weigth[k, i, j])* (weigth[k, i, j]);
                    }
                }
            
                for(int i=0; i < n; i++)
                {
                    sl2 += (x[q, i] - A[i]) * (x[q, i] - A[i]);
                }
                I = M1 * deltat * sl1 + M2 * sl2;
            }
            return I;
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

            SetLastValX(ref x, ref x1);
            SetLastValWeigth(ref weigth, ref weigth1);
            double tempI = Step4_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
            while (!(Math.Abs(tempI - I1) < E))
            {
                if (tempI > I1)
                {
                    Alfa /= 2;
                    tempI = Step5_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
                }
                else
                {
                    textBox7.Text += (Convert.ToString(tempI));
                    I1 = tempI;
                    SetLastValX(ref x, ref x1);
                    SetLastValWeigth(ref weigth, ref weigth1);
                    tempI = Step4_7(ref p, ref x, ref x1, ref weigth, ref weigth1, ref A, ref gamma);
                    
                }
            }
            textBox7.Text += (Convert.ToString(tempI));
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // добавил Delt - дель
            try
            {
                n = Convert.ToInt32(textBox1.Text);
                q = Convert.ToInt32(textBox6.Text);
                T = Convert.ToInt32(textBox2.Text);
                B = Convert.ToDouble(textBox3.Text);
                M1 = Convert.ToDouble(textBox4.Text);
                M2 = Convert.ToDouble(textBox5.Text);
                E = Convert.ToDouble(textBox9.Text);
                Alfa = Convert.ToDouble(textBox8.Text);

            }
            catch (Exception exc)
            {
                MessageBox.Show("Введите данные заново!");
                return;
                
            }


            // то
            DataTable table = new DataTable();

            table.Columns.Add(" ", typeof(string));

            for (int i = 0; i < n; i++)
            {
                table.Columns.Add(Convert.ToString(i + 1), typeof(int));
            }
            table.Rows.Add("ai");
            table.Rows.Add("Ai");
            table.Rows.Add("Гi");

            dataGridView1.DataSource = table;
            dataGridView1.AutoResizeColumns();





         



         
            //for(int i=0; i<n+1; i++) //до n+1, т.к дополнительно введен столбец, в котором написали название ai, Ai, Xi
            //{
            //    dataGridView1.Columns[i].Width = 30;
            //}

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

        private void button3_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show(Convert.ToString(dataGridView2.CurrentRow.Cells[0].Value));
        }
    }
}
