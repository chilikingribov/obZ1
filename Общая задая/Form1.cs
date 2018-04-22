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
using System.Threading;
namespace Общая_задая
{
    public partial class Form1 : Form
    {
        DataTable table;

        NeuroNetwork Network;
        public Form1()
        {
            InitializeComponent();
            Network = new NeuroNetwork();
        }


        private void SetNewParamsAndStartMainLoop(object sender, EventArgs e) // Для инициализации
        {
            if (!Network.InitX(ref ParamsGrid, 0))
                MessageBox.Show("Введите Xi в таблицу");
            if (!Network.InitEndPoints(ref ParamsGrid, 1))
                MessageBox.Show("Введите Ai в таблицу");
            if (!Network.InitOscillation(ref ParamsGrid, 2))
                MessageBox.Show("Введите Wi в таблицу");

            Network.MainLoop();
            MessageBox.Show("The end is now!");
        }

        private void buttonDraw_X_Values(object sender, EventArgs e)
        {
            // Считываем с формы требуемые значения

            double Xmin = 1;

            double Xmax = Network.q;

            double Step = 1;

            double[] Ox = new double[Network.q];
            for (int i = 0; i < Network.q; i++)
                Ox[i] = i + 1;

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
            for (int i = 0; i < Network.Neurons; i++)
            {

                System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
                series.ChartArea = "ChartArea1";
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series.Legend = "Legend1";
                series.Name = "X" + Convert.ToString(i);
                this.chart1.Series.Add(series);
            }

            // Добавляем вычисленные значения в графики

            for (int i = 0; i < Network.Neurons; i++)
            {
                double[] tempX = new double[Network.q];
                for (int j = 0; j < Network.q; j++)
                    tempX[j] = Network.CurrentX[j][i];
                chart1.Series[i].Points.DataBindXY(Ox, tempX);
            }
        }

        private void buttonDraw_P_Values(object sender, EventArgs e)
        {
            // Считываем с формы требуемые значения

            double Pmin = 1;

            double Pmax = Network.q;

            double Step = 1;

            double[] Ox = new double[Network.q];
            for (int i = 0; i < Network.q; i++)
                Ox[i] = i + 1;

            //Значения иксов на последней итерации

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
            for (int i = 0; i < Network.Neurons; i++)
            {

                System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
                series.ChartArea = "ChartArea2";
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
                series.Legend = "Legend2";
                series.Name = "P" + Convert.ToString(i);
                this.chart2.Series.Add(series);
            }

            // Добавляем вычисленные значения в графики

            for (int i = 0; i < Network.Neurons; i++)
            {
                double[] tempP = new double[Network.q];
                for (int j = 0; j < Network.q; j++)
                    tempP[j] = Network.CurrentP[j][i];
                chart2.Series[i].Points.DataBindXY(Ox, tempP);
            }
        }

        private void button_Grid_Y_View(object sender, EventArgs e)
        {
            DataTable table = new DataTable();
            for (int i = 0; i < Network.Neurons; i++)
            {
                table.Columns.Add("Y" + Convert.ToString(i), typeof(double)); // задали шапку 
            }

            for (int k = 0; k < Network.q; k++) table.Rows.Add(); // добавляем нужное количество строчек

            dataGridView4.DataSource = table;

            for (int k = 0; k < Network.q; k++)
            {

                for (int j = 0; j <= Network.Neurons - 1; j++)
                {
                    dataGridView4.Rows[k].Cells[j].Value = Network.CurrentY[k][j];
                }


            }
            dataGridView4.AutoResizeColumns();
        }

        private void button_Grid_U_View(object sender, EventArgs e)
        {

            DataTable table = new DataTable();
           

            for (int k = 0; k < Network.q; k++) table.Rows.Add(); // добавляем нужное количество строчек
            dataGridView5.DataSource = table;
            table.Columns.Add("Номер", typeof(double)); // задали шапку 
            table.Columns.Add("U" , typeof(double)); // задали шапку

            for (int k = 0; k < Network.q; k++)
            {
                dataGridView5.Rows[k].Cells[0].Value = k;
                dataGridView5.Rows[k].Cells[1].Value = Network.CurrentU[k];
            }
            dataGridView5.AutoResizeColumns();
           
        }

        private void button_Grid_X_View(object sender, EventArgs e)
        {
            DataTable table = new DataTable();

            for (int i = 0; i < Network.Neurons; i++)
            {
                table.Columns.Add("X" + Convert.ToString(i), typeof(double)); // задали шапку 
            }

            for (int k = 0; k < Network.q; k++) table.Rows.Add(); // добавляем нужное количество строчек

            dataGridView3.DataSource = table;
           
            for (int k = 0; k < Network.q; k++)
            {

                for (int j = 0; j <= Network.Neurons - 1; j++)
                {
                    dataGridView3.Rows[k].Cells[j].Value=Network.CurrentX[k][j];
                }
            }
            dataGridView3.AutoResizeColumns();
        }

        private void Button_InitializeParams(object sender, EventArgs e)
        {
            double u;
            try
            {
                Network.Neurons = Convert.ToInt32(textBox1.Text);
                Network.q = Convert.ToInt32(textBox6.Text);
                Network.Time = Convert.ToInt32(textBox2.Text);
                Network.Accuracy = Convert.ToDouble(textBox9.Text);
                Network.descentCoeff = Convert.ToDouble(textBox8.Text);
                Network.CoeffE1 = Convert.ToDouble(textBox10.Text);
                Network.CoeffE2 = Convert.ToDouble(textBox7.Text);
                Network.CoeffBeta = Convert.ToDouble(textBox3.Text);
                Network.WeightsCoeffR1 = Convert.ToDouble(textBox4.Text);
                Network.WeightsCoeffR2 = Convert.ToDouble(textBox5.Text);
                u = Convert.ToDouble(textBox12.Text);
            }

            catch (Exception exc)
            {
                MessageBox.Show("Введите данные заново!");
                return;

            }
            //Network.Neurons = 2; Network.q = 10; T = 1; B = 2; M1 = 2; M2 = 3;Alfa = 0.01; E = 0.001;

            Network.InitializeArrays();

            Network.SetStartU(u);

            DataTable table = new DataTable();

            table.Columns.Add(" ", typeof(string));

            for (int i = 0; i < Network.Neurons; i++)
            {
                table.Columns.Add(Convert.ToString(i + 1), typeof(double));
            }

            table.Rows.Add("Xi(0)");
            //table.Rows.Add("Xi(T)");
            table.Rows.Add("Ai");
            table.Rows.Add("wi");
            ParamsGrid.DataSource = table;
            ParamsGrid.AutoResizeColumns();
        }

        ///??????????What is this???????
        private void button2_Click(object sender, EventArgs e)
        {
            int q, Neurons;
            Neurons = Convert.ToInt32(textBox1.Text);
            q = Convert.ToInt32(textBox6.Text);


            DataTable table1 = new DataTable();

            table1.Columns.Add("W", typeof(string));

            for (int i = 0; i < Neurons; i++)
            {
                table1.Columns.Add(Convert.ToString(i + 1), typeof(int));
                table1.Rows.Add(Convert.ToString(i + 1));
            }

            dataGridView2.DataSource = table;
            dataGridView2.AutoResizeColumns();
        }
    }
}
