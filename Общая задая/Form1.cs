using System;
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
        int n = 1, T, B, M1, M2, Alfa, q = 1;
        double E, deltat;
        DataTable table;
        double[,] x;
        double[] A;
        double[,,] weigth;
        double[] gamma;
        private void button4_Click(object sender, EventArgs e)
        {
            // Добавлено новое:
            deltat = T / q;
            int[,,] weigth = new int[q, n, n];
            // Массив X
            double [,] x = new double[q+1, n];
            double[] A = new double[n]; // массив Ai
            double[] gamma = new double[n];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    try
                    {
                        x[0, j] = Convert.ToInt32(dataGridView1.CurrentRow.Cells[j + 1].Value);
           
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Введите ai в таблицу");
                        return;

                    }

                }
            }

            // задание Ai
            for (int i = 0; i < n; i++)
            {
               
                    try
                    {
                    MessageBox.Show(Convert.ToString(dataGridView1.Rows[1].Cells[i + 1].Value));
                    A[i] = Convert.ToInt32(dataGridView1.Rows[1].Cells[i + 1].Value);
                }
                    catch (Exception)
                    {
                        MessageBox.Show("Введите ai в таблицу");
                        return;

                    }

                }
            




            // задаем веса W

            for (int k=0; k<q; k++)
            {
                for(int i=0; i<n; i++)
                {
                    for(int j=0; j<n; j++)
                    {
                        weigth[k, i, j] = 1;
                    }
                }
            }




            Iter0();
            double I = ReturnI();

        }


        private void Iter0() // вычисления на 0 итерации
        {
            // k - слой
            // i, j - нейроны
            // второй тест
            for (int i = 0; i < n; i++)
            {

                double sl2 = 0;// слагаемое первое и слагаемое второе
                for (int k = 0; k < q; k++)
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

        private double ReturnI()
        {
            double I=0, sl1=0, sl2=0;
            for(int k=0; k<q; q++)
            {
                for(int i=0; i < n; i++)
                {
                    for(int j=0; j<n; j++)
                    {
                        sl1 += (weigth[k, i, j])* (weigth[k, i, j]);
                    }
                }
            
            for(int i=0; i < n; i++){
                    sl2 += (x[q - 1, i] - A[i]) * (x[q - 1, i] - A[i]);
                }
                I = M1 * deltat * sl1 + M2 * sl2;
            }
            return I;
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            // добавил Delt - дель
            try
            {
                n = Convert.ToInt32(textBox1.Text);
                q = Convert.ToInt32(textBox6.Text);
                T = Convert.ToInt32(textBox2.Text);
                B = Convert.ToInt32(textBox3.Text);
                M1 = Convert.ToInt32(textBox4.Text);
                M2 = Convert.ToInt32(textBox5.Text);
                E = Convert.ToInt32(textBox9.Text);
                Alfa = Convert.ToInt32(textBox8.Text);

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
            table.Rows.Add("Xi");

            dataGridView1.DataSource = table;
            dataGridView1.AutoResizeColumns();





         



         
            //for(int i=0; i<n+1; i++) //до n+1, т.к дополнительно введен столбец, в котором написали название ai, Ai, Xi
            //{
            //    dataGridView1.Columns[i].Width = 30;
            //}

        }

        private bool inputai()
        {
            //while()
            //for (int j = 0; j < n; j++)
            //{
            //    x[0, j] = Convert.ToInt32(dataGridView1.CurrentRow.Cells[j + 1].Value);
            //}
            return true;
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
