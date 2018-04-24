using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Общая_задая
{
    class NeuroNetwork
    {
        public NeuroNetwork()
        {
            Neurons = 3;
            q = 100;
            Time = 5;
            Accuracy = 0.001;
            descentCoeff = 100;
            CoeffE1 = 1;
            CoeffE2 = 3;
            CoeffBeta = 0.0001;
            WeightsCoeffR1 = 0;
            WeightsCoeffR2 = 10;
        }

        public void InitArrayBySingleVal(ref List<double> array_, double val_, int rows_ = -1)
        {
            array_.Clear();
            int size = rows_ == -1 ? array_.Count : rows_;
            for (int i = 0; i < size; i++)
                array_.Add(val_);
        }

        public void InitArrayBySingleVal(ref List<List<double> > array_, double val_, int rows_ = -1, int columns_ = -1)
        {
            int sizeR = rows_ == -1 ? array_.Count : rows_;
            for (int i = 0; i < sizeR; i++)
            {
                int sizeC = columns_ == -1 ? array_[i].Count : columns_;
                List<double> list;
                if (columns_ != -1)
                {
                    list = new List<double>(sizeC);
                    array_.Add(list);
                }
                for (int j = 0; j < sizeC; j++)
                    array_[i].Add(val_);
            }
        }

        public void SetListValues(ref List<double> list_, double val_, int count_)
        {
            for (int i = 0; i < count_; i++)
                list_[i] = val_;
        }

        public void SetStartU(double val_)
        {
            SetListValues(ref m_CurrentU, val_, 3);
        }

        public void CopyList(ref List<List<double> > srcList_, ref List<List<double> > dstList_)
        {
            for (int i = 0; i < srcList_.Count; i++)
                dstList_[i] = srcList_[i];
        }

        /**
           * @brief Вычисляет новые значения для P и G
           * @return ничего не возвращает
           * @note Для вычислений используются текущие значения X и U
           */
           public void Calc_NewP_And_NewG()
        {
            double sum = 0;
            for (int i = 0; i < m_Neurons; i++)
            {
                m_CurrentP[m_q][i] = -2 * m_WeightsCoeffR2 * (m_CurrentX[m_q][i] - m_EndPoints[i]);   /// Нужно поменять по _
                m_CurrentG[m_q][i] = 0;
            }

            for (int k = m_q - 1; k >= 1; k--)  /// ТТТТТТТТТТТТТТУУУУТ ПРАВИЛ c m_q-1 на m_q
                for (int i = 0; i < m_Neurons; i++)
                {
                    m_CurrentP[k][i] = m_CurrentP[k + 1][i] - m_CurrentG[k + 1][i] * m_delta * (Math.Pow(m_oscillation[i], 2) + 2 * m_CoeffE1 * m_CoeffBeta * m_CurrentX[k][i]);
                    sum = 0;
                    for (int j = 0; j < m_Neurons; j++)
                    {
                        sum += m_CurrentG[k + 1][j];
                    }
                    sum = (double)1 / m_Neurons * sum;
                    m_CurrentG[k][i] = m_delta * m_CurrentP[k + 1][i] + m_CurrentG[k + 1][i] - m_delta * m_CoeffE2 * m_CurrentU[k] * (m_CurrentG[k + 1][i] - sum);
                }
        }

        /**
           * @brief Вычисляем градиент и корректируем значения U
           * @param ValY_ - нужный массив значений Y(зависит от того, какой результат сравнения текущего и предыдущего значений целевой функции)
           * @return ничего не возвращает
           * @note Для вычислений используется <b>переданные</b> значения Y и <b>предыдущее</b> значение U. Текущее значение U пересчитывается
           */
        public void ImproveU(ref List<List<double> > ValY_)
        {
            double firstSum = 0, secondSum = 0, thirdSum = 0;
            for (int k = 0; k < m_q; k++)
            {
                firstSum = 0;
                secondSum = 0;
                for (int i = 0; i < m_Neurons; i++)
                {
                    thirdSum = 0;
                    for (int j = 0; j < m_Neurons; j++)
                    {
                        thirdSum += ValY_[k][j] - ValY_[k][i];
                    }
                    secondSum += thirdSum * m_CurrentG[k + 1][i];

                }
                secondSum *= (double)m_delta * m_CoeffE2 / m_Neurons;
                firstSum = m_WeightsCoeffR1 * m_delta * m_PreviousU[k] - secondSum; // m_CurrentU[k]
                m_CurrentU[k] = m_PreviousU[k] - m_descentCoeff * firstSum;
            }
        }
        
        /**
           * @brief Вычисляет новые значения для X и Y
           * @return ничего не возвращает
           * @note Для вычисления используются текущие значения U. X и Y пересчитываются
           */
           public void Calc_NewX_And_NewValY()
        {
            // k - слой
            // i, j - нейроны
            double doubleSum = 0;
            
            for (int k = 0; k < m_q; k++)
            {
                for (int i = 0; i < m_Neurons; i++)
                {
                    m_CurrentX[k + 1][i] = m_CurrentX[k][i] + m_delta * m_CurrentY[k][i];
                    doubleSum = 0;
                    for (int j = 0; j < m_Neurons; j++)
                    {
                        doubleSum += m_CurrentY[k][j] - m_CurrentY[k][i];
                    }
                    doubleSum = doubleSum * m_CoeffE2 * m_CurrentU[k];
                    doubleSum = (double)doubleSum / m_Neurons;
                    m_CurrentY[k + 1][i] = m_CurrentY[k][i] + m_delta * (-Math.Pow(m_oscillation[i], 2) * m_CurrentX[k][i] + m_CoeffE1 * (1 - m_CoeffBeta * Math.Pow(m_CurrentX[k][i], 2)) + doubleSum);
                }
            }
        }

        /**
           * @brief Считает значение целевой функции
           * @return Значение целевой функции
           * @note Для вычислений используются текущие значения X и U
           */
        public double Calc_TargetFuncVal()
        {
            double FuncVal = 0, firstSum = 0, secondSum = 0;
            for (int k = 0; k < m_q; k++)
            {
                firstSum += m_CurrentU[k] * m_CurrentU[k];
                //firstSum = (double)firstSum / 2;
            }
            firstSum *= m_WeightsCoeffR1 * m_delta*0.5;
            for (int i = 0; i < m_Neurons; i++)
            {
                secondSum += Math.Pow((m_CurrentX[m_q][i] - m_EndPoints[i]), 2);
            }
            secondSum *= m_WeightsCoeffR2;
            FuncVal = firstSum + secondSum;
            return FuncVal;
        }

        /**
           * @brief Вычисляет значение целевой функции в случае, когда текущее значение было меньше предыдущего(уменьшилось)
           * @return значение целевой функции
           * @note Сначала считает P и G, затем улучшаем U, используя текущий Y, потом вычисляем X и Y и наконец вычисляем значение функции
           */
           public double CalcFuncIfCurrValDecreased()
        {
            //Вычисляем новые значения P и G
            Calc_NewP_And_NewG();
            //Считаем градиент, улучшаем управление
            ImproveU(ref m_CurrentY);
            //Вычисляем новые значения для X и Y
            Calc_NewX_And_NewValY();
            //Считаем значение целевой функции
            return Calc_TargetFuncVal();
        }

        /**
           * @brief Вычисляет значение целевой функции в случае, когда текущее значение было больше предыдущего(увеличилось)
           * @return значение целевой функции
           * @note Сначала улучшаем U, используя значение Y на предыдущем шаге, потом вычисляем X и Y и наконец вычисляем значение функции
           */
        public double CalcFuncIfCurrValIncreased()
        {
            //Считаем градиент, улучшаем управление
            ImproveU(ref m_PreviousY);
            //Вычисляем новые значения для X и Y
            Calc_NewX_And_NewValY();
            //Считаем значение целевой функции
            return Calc_TargetFuncVal();
        }

        /**
           * @brief Осуществляет основные вычисление
           * @return возвращает признак успешного завершения работы(true - функция посчитана с заданной точностью, false - ошибка)
           * @note сохраняет два списка значений - один со убывающими значениями и один со значениями при возрастании функции
           */
           public bool MainLoop()
        {
            m_delta = (double)m_Time / m_q;
            listOfTargetFuncValues.Clear();
            listOfBadTrgFuncValues.Clear();
            

            m_PrevValOfTargetFunc = Calc_FirstVal_Of_TargetFunc();
            listOfTargetFuncValues.Add(m_PrevValOfTargetFunc);

            CopyLastVals_X_Y_U();
            m_CurrValOfTargetFunc = CalcFuncIfCurrValDecreased();

            if (m_CurrValOfTargetFunc < m_PrevValOfTargetFunc)
                listOfTargetFuncValues.Add(m_CurrValOfTargetFunc);
            double Val_Of_TgtFunc_InLoop = m_PrevValOfTargetFunc;

            
            while (!(Math.Abs(m_CurrValOfTargetFunc - m_PrevValOfTargetFunc) < m_accuracy) && !double.IsNaN(m_CurrValOfTargetFunc) && !double.IsNaN(m_PrevValOfTargetFunc))
            {
                if (m_CurrValOfTargetFunc > m_PrevValOfTargetFunc)
                {
                    m_descentCoeff /= 2;
                    if (Math.Abs(m_CurrValOfTargetFunc - Val_Of_TgtFunc_InLoop) < (double)m_accuracy / 10)
                    {
                        return false;
                    }
                    Val_Of_TgtFunc_InLoop = m_CurrValOfTargetFunc;
                    m_CurrValOfTargetFunc = CalcFuncIfCurrValIncreased();
                    listOfBadTrgFuncValues.Add(m_CurrValOfTargetFunc);
                }
                else
                {
                    m_PrevValOfTargetFunc = m_CurrValOfTargetFunc;
                    CopyLastVals_X_Y_U();
                    m_CurrValOfTargetFunc = CalcFuncIfCurrValDecreased();
                    listOfTargetFuncValues.Add(m_CurrValOfTargetFunc);
                }
            }
            return true;
        }

        /**
           * @brief Используем для вычисления начального значения целевой функции
           * @return значение целевой функции
           * @note Вычисляются X и Y и значение целевой цункции
           */
           private double Calc_FirstVal_Of_TargetFunc()
        {
            Calc_NewX_And_NewValY();
            return Calc_TargetFuncVal();
        }

        /**
           * @brief Копирует текущие значения X, Y, U.
           * @return ничего не возвращает
           * @note запоминаем значения не предыдущей итерации
           */
           private void CopyLastVals_X_Y_U()
        {
            CopyList(ref m_CurrentX, ref m_PreviousX);
            CopyList(ref m_CurrentY, ref m_PreviousY);
            m_PreviousU = m_CurrentU;
        }

        ///Количество нейронов
        private int m_Neurons;//n
        public int Neurons
        {
            get
            {
                return m_Neurons;
            }
            set
            {
                m_Neurons = value;
            }
        }

        /// момент времени
        private int m_Time;//T
        public int Time
        {
            get
            {
                return m_Time;
            }
            set
            {
                m_Time = value;
            }
        }


        ///Точность аппроксимации
        private int m_q; //q
        public int q
        {
            get
            {
                return m_q;
            }
            set
            {
                m_q = value;
            }
        }

        ///точность вычислений
        private double m_accuracy; //E
        public double Accuracy
        {
            get
            {
                return m_accuracy;
            }
            set
            {
                m_accuracy = value;
            }
        }

        ///Отношение момента времени к точности аппроксимации(m_Time/m_q)
        private double m_delta;//deltat
        public double delta
        {
            get
            {
                return m_delta;
            }
            set
            {
                m_delta = value;
            }
        }


        ///Коэффициент градиентного спуска
        private double m_descentCoeff;//Alfa
        public double descentCoeff
        {
            get
            {
                return m_descentCoeff;
            }
            set
            {
                m_descentCoeff = value;
            }
        }

        ///коэффициенты, характеризующие нелинейное воздействие на i-нейрон
        private double m_CoeffE1;//E1
        public double CoeffE1
        {
            get
            {
                return m_CoeffE1;
            }
            set
            {
                m_CoeffE1 = value;
            }
        }
        private double m_CoeffE2;//E2
        public double CoeffE2
        {
            get
            {
                return m_CoeffE2;
            }
            set
            {
                m_CoeffE2 = value;
            }
        }
        private double m_CoeffBeta;//Bet
        public double CoeffBeta
        {
            get
            {
                return m_CoeffBeta;
            }
            set
            {
                m_CoeffBeta = value;
            }
        }

        ///весовые коэффициенты в двухкритериальной задаче оптимального управления
        private double m_WeightsCoeffR1;//R1
        public double WeightsCoeffR1
        {
            get
            {
                return  m_WeightsCoeffR1;
            }
            set
            {
                m_WeightsCoeffR1 = value;
            }
        }
        private double m_WeightsCoeffR2;//R2
        public double WeightsCoeffR2
        {
            get
            {
                return m_WeightsCoeffR2;
            }
            set
            {
                m_WeightsCoeffR2 = value;
            }
        }

        ///собственная частота колебаний i-нейрона
        private List<double> m_oscillation;//w
        public List<double> Oscillation
        {
            get
            {
                return m_oscillation;
            }
            set
            {
                m_oscillation = value;
            }
        }

        ///Точки, в которые нужно прийти из начальных
        private List<double> m_EndPoints;//A
        public List<double> EndPoints
        {
            get
            {
                return m_EndPoints;
            }
            set
            {
                m_EndPoints = value;
            }
        }

        ///Текущее значение управления
        private List<double> m_CurrentU;//u
        public List<double> CurrentU
        {
            get
            {
                return m_CurrentU;
            }
            set
            {
                m_CurrentU = value;
            }
        }

        ///Значение управления на предыдущем шаге итерации
        private List<double> m_PreviousU;//u1
        public List<double> PreviousU
        {
            get
            {
                return m_PreviousU;
            }
            set
            {
                m_PreviousU = value;
            }
        }

        ///Текущее значение амплитуд колебаний i-нейрона(X)
        private List<List<double> > m_CurrentX;//x
        public List<List<double> > CurrentX
        {
            get
            {
                return m_CurrentX;
            }
            set
            {
                m_CurrentX = value;
            }
        }

        ///Значение X на предыдущем шаге итерации
        private List<List<double> > m_PreviousX;//x1
        public List<List<double> > PreviuosX
        {
            get
            {
                return m_PreviousX;
            }
            set
            {
                m_PreviousX = value;
            }
        }

        ///Текущее значение скорости изменения амплитуды колебаний i-нейрона(Y)
        private List<List<double> > m_CurrentY;//y
        public List<List<double>> CurrentY
        {
            get
            {
                return m_CurrentY;
            }
            set
            {
                m_CurrentY = value;
            }
        }

        ///Значение Y на предыдущем шаге итерации
        private List<List<double> > m_PreviousY;//y1
        public List<List<double>> PreviousY
        {
            get
            {
                return m_PreviousX;
            }
            set
            {
                m_PreviousX = value;
            }
        }

        ///Текущее значение g
        private List<List<double> > m_CurrentG;//g
        public List<List<double>> CurrentG
        {
            get
            {
                return m_CurrentG;
            }
            set
            {
                m_CurrentG = value;
            }
        }
        //private List<List<double> > m_LastG;//g1

        ///Текущее значение p
        private List<List<double>> m_CurrentP;//p
        public List<List<double>> CurrentP
        {
            get
            {
                return m_CurrentP;
            }
            set
            {
                m_CurrentP = value;
            }
        }
    //private List<List<double> > m_LastP;//p1

    ///Значение функции на текущей итерации
    private double m_CurrValOfTargetFunc { get; set; }
        ///Значение функции на предыдущей итерации
        private double m_PrevValOfTargetFunc { get; set; }

        ///Список значений целевой функции содержащий её значения по убыванию
        List<double> listOfTargetFuncValues = new List<double>();

        public List<double> getListTrgVals()
        {
            return listOfTargetFuncValues;
        }

        ///Список значений целевой функции содержащий её значения когда уменьшался шаг градиентого спуска(то есть функция увеличивалась по сравнению с предыдущим значением)
        List<double> listOfBadTrgFuncValues = new List<double>();

        public List<double> getListBadTrgVals()
        {
            return listOfBadTrgFuncValues;
        }

        /**
           * @brief Создаёт массивы
           */
           public void InitializeArrays()
        {
            m_oscillation = new List<double>(m_Neurons);
            m_EndPoints = new List<double>(m_Neurons);
            m_CurrentU = new List<double>(m_q);
            m_PreviousU = new List<double>(m_q);
            m_CurrentX = new List<List<double>>(m_q + 1);
            m_PreviousX = new List<List<double>>(m_q + 1);
            m_CurrentY = new List<List<double>>(m_q + 1);
            m_PreviousY = new List<List<double>>(m_q + 1);
            m_CurrentG = new List<List<double>>(m_q + 1);
            m_CurrentP = new List<List<double>>(m_q + 1);
            InitArrayBySingleVal(ref m_oscillation, 0, m_Neurons);
            InitArrayBySingleVal(ref m_EndPoints, 0, m_Neurons);
            InitArrayBySingleVal(ref m_PreviousU, 0, m_q);
            InitArrayBySingleVal(ref m_CurrentX, 0, m_q + 1, m_Neurons);
            InitArrayBySingleVal(ref m_PreviousX, 0, m_q + 1, m_Neurons);
            InitArrayBySingleVal(ref m_CurrentY, 0, m_q + 1, m_Neurons);
            for (int i = 0; i < m_CurrentY[0].Count; i++)
                m_CurrentY[0][i] = 1;
            InitArrayBySingleVal(ref m_PreviousY, 0, m_q + 1, m_Neurons);
            InitArrayBySingleVal(ref m_CurrentG, 0, m_q + 1, m_Neurons);
            InitArrayBySingleVal(ref m_CurrentP, 0, m_q + 1, m_Neurons);
        }


        /**
           * @brief Заполняет лист значениями double выбранными из строки грида
           * @param list_ - контейнер, в который считываются данные
           * @param grid_ - грид, из которого читаются данные
           * @param row_ - строка, из которой читабются данные
           */
        public bool InitArray(ref List<double> list_, ref System.Windows.Forms.DataGridView grid_, int row_)
        {
            list_.Clear();
            int columns = grid_.ColumnCount;
            // задание Ai
            for (int i = 1; i < columns; i++)
            {

                try
                {
                    list_.Add(Convert.ToDouble(grid_.Rows[row_].Cells[i].Value));
                }
                catch (Exception)
                {
                    return false;

                }
            }
            return true;
        }

        /**
           * @brief Заполняет лист значениями double выбранными из строки грида
           * @param list_ - контейнер, в который считываются данные
           * @param grid_ - грид, из которого читаются данные
           */
        public bool InitArray(ref List<List<double>> list_, ref System.Windows.Forms.DataGridView grid_)
        {
            int columns = grid_.ColumnCount;
            int rows = grid_.RowCount;
            list_.Clear();
            // задание Ai
            for (int i = 1; i < columns; i++)
            {
                List<double> array = new List<double>(rows);
                list_.Add(array);
                for (int j = 1; j < rows; j++)
                {
                    try
                    {
                        list_[i - 1].Add(Convert.ToDouble(grid_.Rows[j].Cells[i].Value));
                    }
                    catch (Exception)
                    {
                        return false;

                    }
                }
            }
            return true;
        }

        public bool InitEndPoints(ref System.Windows.Forms.DataGridView grid_, int row_)
        {
            return InitArray(ref m_EndPoints, ref grid_, row_);
        }

        public bool InitU(ref System.Windows.Forms.DataGridView grid_, int row_)
        {
            return InitArray(ref m_CurrentU, ref grid_, row_);
        }

        public bool InitU(double value_)
        {
            InitArrayBySingleVal(ref m_CurrentU, value_, m_q);
            return true;
        }

        public bool InitX(ref System.Windows.Forms.DataGridView grid_, int row_)
        {
            List<double> values = new List<double>();
            bool ok = InitArray(ref values, ref grid_, row_);
            if (!ok)
                return false;
            for (int i = 0; i < values.Count; i++)
                m_CurrentX[0][i] = values[i];
            return true;
        }

        public bool InitOscillation(ref System.Windows.Forms.DataGridView grid_, int row_)
        {
            return InitArray(ref m_oscillation, ref grid_, row_);
        }


    }
}
