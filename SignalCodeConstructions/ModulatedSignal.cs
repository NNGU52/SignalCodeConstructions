using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCodeConstructions
{
    class ModulatedSignal
    {
        public int M;                              // кол-во фаз (индекс модуляции)
        public int beginPhase;                     // начальная фаза сигнала
        public int amplitude;                      // амплитуда сигнала
        public int number_period;                  // кол-во периодов на 1 бит информации
        public int ModLength;                      // длина модулирующего и модулированного сигнала
        public double freqNes;                     // частота несущего сигнала
        public double energy;                      // интенсивность
        public bool b;                             // проверка на нажатие чекбокса
        public int length;                         // длина информационного сигнала (с интерфейса)
        public double snr;

        public List<PointD> data = new List<PointD>();

        public ModulatedSignal(int NumberPhase, int BeginPhaseSignal, int AmplitudeSignal, double FreqNes, double Energy, bool B, int Length)
        {
            M = NumberPhase;
            beginPhase = BeginPhaseSignal;
            amplitude = AmplitudeSignal;
            freqNes = FreqNes;
            energy = Energy;
            b = B;
            length = Length;

            number_period = 3;

        }

        // рассчет фаз
        public double[] CalculateNumberPhase()
        {
            double[] phi = new double[M + 1]; // массив из M значений (фазы)
            for (int k = 1; k <= M; k++)
            {
                if (M == 4)
                {
                    phi[k] = (2 * Math.PI * (k - 1)) / M + beginPhase + (Math.PI / 4);
                }
                else
                {
                    phi[k] = (2 * Math.PI * (k - 1)) / M + beginPhase;
                }
            }

            return phi;
        }

        // расчет позиций фаз
        public void CalculatePosition()
        {
            double[] phi = CalculateNumberPhase();

            int anchorX = 295,
                anchorY = 195,
                ballX,
                ballY;

            for (int i = 1; i <= M; i++)
            {
                ballX = anchorX + (int)(Math.Cos(phi[i]) * 150);
                ballY = anchorY + (int)(Math.Sin(phi[i]) * 150);
                data.Add(new PointD(ballX, ballY));
            }
        }

        // расчет позиций фаз (восстановленное созвездие)
        public List<PointD> CalculatePosition_recovery(List<double> phi)
        {
            List<PointD> data_ = new List<PointD>();
            int anchorX = 295,
                anchorY = 195,
                ballX,
                ballY;

            for (int i = 0; i < phi.Count; i++)
            {
                ballX = anchorX + (int)(Math.Cos(phi[i]) * 150);
                ballY = anchorY + (int)(Math.Sin(phi[i]) * 150);
                data_.Add(new PointD(ballX, ballY));
            }

            return data_;
        }

        // полная энергия сигнала
        double CalculateEnergySignal(double[] y)
        {
            double Es = 0;

            for (int i = 0; i < y.Length; i++)
            {
                Es += Math.Pow(y[i], 2);
            }

            return Es;
        }

        // метод генерации шума
        public double[] GenerateNoise(double[] signal)
        {
            Random rnd = new Random();
            int length = signal.Length;

            // Генерация последовательности нормально распределённых случайных чисел.
            double[] massRand = new double[length];  // Массив случайных чисел. 
            double Er = 0;

            for (int i = 0; i < length; i++)
            {
                massRand[i] = 0;
                for (int n = 0; n < 12; n++)
                {
                    massRand[i] += Convert.ToDouble(rnd.Next(-100, 101)) / 100;  // реализация равномерно распределенной случайной величины, в интервале значений [−1, 1]
                }
                massRand[i] /= 12;
                Er += massRand[i] * massRand[i];
            }

            // Подсчёт энергии шума относительно энергии сигнала.
            double Es = CalculateEnergySignal(signal);

            double Enoise = Es * energy / 100;  // Энергия шума относительно энергии сигнала.

            // Нормировка случайной последовательности.
            for (int i = 0; i < length; i++)
            {
                if (b)
                    massRand[i] = massRand[i] * Math.Sqrt(Enoise / Er);
                else
                    massRand[i] = 0;
            }

            return massRand;

        }

        // метод для подсчета отношения сигнал/шум
        double SNR(double[] noise, double[] signal)
        {
            double snr;
            double En = 0;
            double Es = 0;

            for (int i = 0; i < signal.Length; i++)
            {
                En += Math.Pow(noise[i], 2);
                Es += Math.Pow(signal[i], 2);
            }

            snr = 10 * Math.Log10(Es / En);   // отношение сигнал/шум 

            return snr;

        }

        // расчет точек модулирующего сигнала
        public double[] CalculatePointsModulated(List<int> exit_coder, double counter)
        {
            ModLength = (int)(number_period * counter * exit_coder.Count);
            double[] y1 = new double[ModLength];
            double[] phi = CalculateNumberPhase();
            double step;

            switch (M)
            {
                case 2:
                    step = number_period * counter;
                    for (int i = 0, k = 0; i < exit_coder.Count; i++, k += (int)step)
                    {
                        if (exit_coder[i] == 1)
                        {
                            y1[k] = amplitude * Math.Cos(phi[1]);
                        }
                        if (exit_coder[i] == 0)
                        {
                            y1[k] = amplitude * Math.Cos(phi[2]);
                        }
                    }

                    for (int i = 0; i < y1.Length; i++)
                    {
                        if (y1[i] == 0)
                        {
                            y1[i] = y1[i - 1];
                        }
                    }

                    break;

                case 4:
                    step = number_period * counter * 2;
                    for (int i = 1, k = 0; i < exit_coder.Count; i += 2, k += (int)step)
                    {
                        if (exit_coder[i] == 1)
                        {
                            if (exit_coder[i - 1] == 0)
                                y1[k] = amplitude * Math.Cos(phi[1]);
                            if (exit_coder[i - 1] == 1)
                                y1[k] = amplitude * Math.Cos(phi[4]);

                        }
                        if (exit_coder[i] == 0)
                        {
                            if (exit_coder[i - 1] == 0)
                                y1[k] = amplitude * Math.Cos(phi[2]);
                            if (exit_coder[i - 1] == 1)
                                y1[k] = amplitude * Math.Cos(phi[3]);
                        }
                    }

                    for (int i = 0; i < y1.Length; i++)
                    {
                        if (y1[i] == 0)
                        {
                            y1[i] = y1[i - 1];
                        }
                    }

                    break;

                case 8:
                    step = number_period * counter * 3;
                    for (int i = 2, k = 0; i < exit_coder.Count; i += 3, k += (int)step)
                    {
                        if (exit_coder[i] == 1)
                        {
                            if (exit_coder[i - 1] == 0)
                            {
                                if (exit_coder[i - 2] == 1)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[8]);
                                }
                                if (exit_coder[i - 2] == 0)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[7]);
                                }
                            }
                            if (exit_coder[i - 1] == 1)
                            {
                                if (exit_coder[i - 2] == 1)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[2]);
                                }
                                if (exit_coder[i - 2] == 0)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[1]);
                                }
                            }
                        }
                        if (exit_coder[i] == 0)
                        {
                            if (exit_coder[i - 1] == 0)
                            {
                                if (exit_coder[i - 2] == 0)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[3]);
                                }
                                if (exit_coder[i - 2] == 1)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[4]);
                                }
                            }
                            if (exit_coder[i - 1] == 1)
                            {
                                if (exit_coder[i - 2] == 0)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[5]);
                                }
                                if (exit_coder[i - 2] == 1)
                                {
                                    y1[k] = amplitude * Math.Cos(phi[6]);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < y1.Length; i++)
                    {
                        if (y1[i] == 0)
                        {
                            y1[i] = y1[i - 1];
                        }
                    }

                    break;
            }
            return y1;
        }

        // модулированный сигнал
        public double[] ModernModulatedSignal(List<int> exit_coder, double counter, double[] nes_signal)
        {
            ModLength = (int)(number_period * counter * exit_coder.Count);
            double[] phi = CalculateNumberPhase();
            double[] y2 = new double[ModLength];

            int step;
            int n_star = 0;
            int n_new = 0;
            switch (M)
            {
                case 2:
                    step = (int)(number_period * counter);

                    for (int i = 0; i < exit_coder.Count; i++)
                    {
                        n_new += step;
                        if (n_new <= ModLength)
                        {
                            if (exit_coder[i] == 1)
                            {
                                for (int n = n_star; n < n_new; n++)
                                {
                                    y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[1]);
                                }
                            }
                            if (exit_coder[i] == 0)
                            {
                                for (int n = n_star; n < n_new; n++)
                                {
                                    y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[2]);
                                }
                            }
                        }

                        n_star = n_new;
                    }

                    double[] noise = GenerateNoise(nes_signal);

                    // массив для генерации шума
                    snr = SNR(noise, y2);
                    Console.WriteLine(snr);

                    for (int i = 0; i < ModLength; i++)
                    {
                        y2[i] = noise[i] + y2[i];
                    }

                    break;

                case 4:
                    step = (int)(number_period * counter * 2);

                    for (int i = 1; i < exit_coder.Count; i += 2)
                    {
                        n_new += step;
                        if (n_new <= ModLength)
                        {
                            if (exit_coder[i] == 1)
                            {
                                if (exit_coder[i - 1] == 0)
                                {
                                    for (int n = n_star; n < n_new; n++)
                                    {
                                        y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[1]);
                                    }
                                }
                                if (exit_coder[i - 1] == 1)
                                {
                                    for (int n = n_star; n < n_new; n++)
                                    {
                                        y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[4]);
                                    }
                                }
                            }
                            if (exit_coder[i] == 0)
                            {
                                if (exit_coder[i - 1] == 0)
                                {
                                    for (int n = n_star; n < n_new; n++)
                                    {
                                        y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[2]);
                                    }
                                }
                                if (exit_coder[i - 1] == 1)
                                {
                                    for (int n = n_star; n < n_new; n++)
                                    {
                                        y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[3]);
                                    }
                                }
                            }
                        }

                        n_star = n_new;
                    }

                    double[] noise1 = GenerateNoise(nes_signal);

                    snr = SNR(noise1, y2);

                    for (int i = 0; i < ModLength; i++)
                    {
                        y2[i] = noise1[i] + y2[i];
                    }

                    break;

                case 8:
                    step = (int)(number_period * counter * 3);

                    for (int i = 2; i < exit_coder.Count; i += 3)
                    {
                        n_new += step;
                        if (n_new <= ModLength)
                        {
                            if (exit_coder[i] == 1)
                            {
                                if (exit_coder[i - 1] == 0)
                                {
                                    if (exit_coder[i - 2] == 1)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[8]);
                                        }
                                    }
                                    if (exit_coder[i - 2] == 0)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[7]);
                                        }
                                    }
                                }
                                if (exit_coder[i - 1] == 1)
                                {
                                    if (exit_coder[i - 2] == 1)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[2]);
                                        }
                                    }
                                    if (exit_coder[i - 2] == 0)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[1]);
                                        }
                                    }
                                }
                            }
                            if (exit_coder[i] == 0)
                            {
                                if (exit_coder[i - 1] == 0)
                                {
                                    if (exit_coder[i - 2] == 0)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[3]);
                                        }
                                    }
                                    if (exit_coder[i - 2] == 1)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[4]);
                                        }
                                    }
                                }
                                if (exit_coder[i - 1] == 1)
                                {
                                    if (exit_coder[i - 2] == 0)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[5]);
                                        }
                                    }
                                    if (exit_coder[i - 2] == 1)
                                    {
                                        for (int n = n_star; n < n_new; n++)
                                        {
                                            y2[n] = amplitude * Math.Cos((2 * Math.PI * freqNes * n) + beginPhase + phi[6]);
                                        }
                                    }
                                }
                            }
                        }

                        n_star = n_new;
                    }

                    double[] noise2 = GenerateNoise(nes_signal);

                    snr = SNR(noise2, y2);

                    for (int i = 0; i < ModLength; i++)
                    {
                        y2[i] = noise2[i] + y2[i];
                    }

                    break;
            }

            return y2;
        }

        // метод дополняющий длину модулирующего сигнала до длины 2^N
        public double[] ModernModulated(double[] modulated_signal, double[] nes_signal)
        {
            double[] buf_signal = new double[length];

            for (int i = 0; i < modulated_signal.Length; i++)
            {
                buf_signal[i] = modulated_signal[i];
            }

            for (int i = 0; i < length; i++)
            {
                if (buf_signal[i] == 0)
                {
                    buf_signal[i] = amplitude * Math.Cos((2 * Math.PI * freqNes * i) + beginPhase);
                }
            }

            double[] noise2 = GenerateNoise(nes_signal);

            for (int i = 0; i < buf_signal.Length; i++)
            {
                buf_signal[i] = noise2[i] + buf_signal[i];
            }


            return buf_signal;

        }

        // демодуляция(шаг №1) произведение модулированного сигнала на несущую
        public double[] DemodulationStep1(List<int> exit_coder, double counter, double[] signal_nes)
        {
            double[] y3 = new double[ModLength];
            double[] y2 = ModernModulatedSignal(exit_coder, counter, signal_nes);
            
            for (int i = 0; i < ModLength; i++)
            {
                y3[i] = signal_nes[i] * y2[i];
            }

            return y3;
        }

        // метод генерации точек сглаживания
        public double[] Smooth(double[] input, int window)
        {
            if (window % 2 == 0)                        // проверка: размер окна четный или нет
                window += 1;

            int hw = (window - 1) / 2;                  // размах окна влево и вправо от текущей позиции
            int n = input.Length;
            double[] result = new double[n];

            for (int i = 0; i < n; i++)                 // организуем цкл по числу элементов
            {
                double init_sum = 0;
                int k1, k2, z;

                if (i <= hw)                            // если индекс меньше половины, мы находимся в начале массива
                {
                    k1 = 1;                             // в качестве начала окна мы берем первый элемент
                    k2 = window;                        // конец окна
                    z = window;                         // текущий размер окна
                }
                else
                {
                    if (i + hw > n)                     // если индекс + половина окна больше n, мы приближаемся к концу массива и размер окна также нужно уменьшать
                    {
                        result[i] = result[i - 1];
                        continue;
                    }
                    else                                // иначе мы в середине массива
                    {
                        k1 = i - hw;
                        k2 = i + hw;
                        z = window;
                    }
                }

                for (int j = k1; j < k2; j++)           // организуем цикл от начала до конца окна
                    init_sum += input[j];               // складываем все элементы

                result[i] = init_sum / z;               // и делим на текущий размер окна
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i] / 60;
            }

            return result;
        }
    }
}
