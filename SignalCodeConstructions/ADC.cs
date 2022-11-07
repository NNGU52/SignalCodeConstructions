using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalCodeConstructions
{
    class ADC
    {
        public int inf_length;   // длина информационногосигнала
        public int N;           // кол-во ступеней при квантовании
        public int amplitude;   // амплитуда сигнала (всех)
        public int q;           // шаг квантования
        public double n;           // разрядность квантования (глубина)
        public double freqNes;  // частота несущего сигнала

        // конструктор класса
        public ADC(int InfLength, int NumberStep, int AmplitudeSignal, double FreqNes)
        {
            inf_length = InfLength;
            N = NumberStep;
            amplitude = AmplitudeSignal;
            freqNes = FreqNes;

            N = N - 1;                               // для корректного квантования
            n = Math.Log(N, 2.0);
            q = (amplitude - (-1 * amplitude)) / N;  // шаг квантования
        }

        // метод считает значения уровней квантования
        public double[] ValueUroven()
        {
            double[] value_uroven = new double[N + 1];  // массив со значениями уровней
            double[] quant = new double[inf_length];

            double uroven = amplitude;
            for (int i = 0, k = 0; i <= N; i++, k += q)
            {
                value_uroven[i] = uroven;
                uroven = uroven - q;
            }

            return value_uroven;
        }

        // метод, который квантует сигнал
        public double[] Quantization(double[] signal)
        {
            double[] quant = new double[signal.Length];
            double[] value_uroven = ValueUroven();

            for (int i = 0; i < signal.Length; i++)
            {
                for (int k = 0; k < value_uroven.Length; k++)
                {
                    if (signal[i] <= value_uroven[k] + (q / 2) && signal[i] > value_uroven[k] - (q / 2))
                    {
                        quant[i] = value_uroven[k];
                    }
                }
            }

            BeginCoder();

            return quant;
        }

        // бинарное представление числа
        private List<int> BinaryValue(int value)
        {
            List<int> binary_value = new List<int>();
            int chislo_ = value;

            while (chislo_ != 0)
            {
                int mod;
                mod = chislo_ % 2;
                chislo_ = chislo_ / 2;
                binary_value.Insert(0, mod);
                if (chislo_ == 1 && mod == 1)
                {
                    binary_value.Insert(0, chislo_);
                    break;
                }
            }

            while (binary_value.Count < n)
            {
                binary_value.Insert(0, 0);
            }

            return binary_value;
        }

        // кодер
        public List<int> Coder(List<int> signal)
        {
            List<int> exit_coder = new List<int>();
            List<int> registr_memory = new List<int>();
            List<int> buf_last = new List<int>();
            List<int> buf_simple = new List<int>();

            // изначально регистр памяти заполнен нулями (00)
            registr_memory.Add(0);
            registr_memory.Add(0);

            for (int i = 0; i < signal.Count; i++)
            {
                // копируем элементы
                buf_last.Add(registr_memory[0]);
                buf_last.Add(registr_memory[1]);

                registr_memory.Remove(registr_memory.Last());
                registr_memory.Insert(0, signal[i]);

                buf_simple.Add(registr_memory[0]);
                buf_simple.Add(registr_memory[1]);

                if (buf_last[0] == 0 && buf_last[1] == 0 && buf_simple[0] == 1 && buf_simple[1] == 0)
                {
                    exit_coder.Add(1);
                    exit_coder.Add(1);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 1 && buf_last[1] == 0 && buf_simple[0] == 1 && buf_simple[1] == 1)
                {
                    exit_coder.Add(1);
                    exit_coder.Add(0);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 1 && buf_last[1] == 1 && buf_simple[0] == 0 && buf_simple[1] == 1)
                {
                    exit_coder.Add(1);
                    exit_coder.Add(0);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 0 && buf_last[1] == 1 && buf_simple[0] == 0 && buf_simple[1] == 0)
                {
                    exit_coder.Add(1);
                    exit_coder.Add(1);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 0 && buf_last[1] == 0 && buf_simple[0] == 0 && buf_simple[1] == 0)
                {
                    exit_coder.Add(0);
                    exit_coder.Add(0);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 1 && buf_last[1] == 1 && buf_simple[0] == 1 && buf_simple[1] == 1)
                {
                    exit_coder.Add(0);
                    exit_coder.Add(1);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 0 && buf_last[1] == 1 && buf_simple[0] == 1 && buf_simple[1] == 0)
                {
                    exit_coder.Add(0);
                    exit_coder.Add(0);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
                if (buf_last[0] == 1 && buf_last[1] == 0 && buf_simple[0] == 0 && buf_simple[1] == 1)
                {
                    exit_coder.Add(0);
                    exit_coder.Add(1);

                    buf_simple.Clear();
                    buf_last.Clear();
                    continue;
                }
            }

            return exit_coder;
        }

        // метод возвращает то, что уже закодировано сверточным кодером со скоростью кодирования 1/2
        public List<int> BeginCoder()
        {
            List<int> binary_value = new List<int>();
            List<int> exit_coder = new List<int>();
            List<int> exit_coder_ = new List<int>();

            for (int i = 0; i <= N; i++)
            {
                binary_value = BinaryValue(i);
                exit_coder = Coder(binary_value);
                for (int k = 0; k < exit_coder.Count; k++)
                {
                    exit_coder_.Add(exit_coder[k]);
                }
            }

            return exit_coder_;
        }

        // расчет значения 1 отсчета (n)
        public int CalculateValueOneReference(double[] signal, List<int> exit_coder)
        {
            double value;
            int colvo_period;
            // для начала рассчитаем за какое время будет полный оборот(1 период сигнала)
            value = 1 / freqNes; // получим не в СИ(у нас частота в МГц, следовательно время получим в микросекундах)
            int counter = 1;
            for (int i = 1; i < signal.Length; i++)
            {
                counter++;
                if (signal[i] == amplitude)
                {
                    break;
                }
            }

            value = counter / value;   // получим время в микросекундах одного отсчета
            colvo_period = (int)(freqNes * (inf_length * value));

            if (colvo_period < exit_coder.Count * number_period)
            {
                MessageBox.Show("Выберите кол-во периодов на 1 бит меньше или измените длину/частоту несущего сигнала!");
            }

            return counter;
        }
    }
}
