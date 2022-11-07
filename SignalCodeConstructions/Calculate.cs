using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections;
using System.Windows.Forms;

namespace SignalCodeConstructions
{
    struct PointD
    {
        public int x;
        public int y;

        public PointD(int x_, int y_)
        {
            this.x = x_;
            this.y = y_;
        }
    }

    class Calculate
    {
        int amplitude;             // амплитуда сигнала
        double freq;               // частота сигнала(информационного)
        double freqNes;            // частота сигнала несущего
        double beginPhase;         // начальная фаза
        public int Length;         // длина сигнала

        // конструктор
        public Calculate(int SignalAmplitude, double SignalFreq, double BeginSignalPhase, int SignalLength, double FreqNes)
        {
            amplitude = SignalAmplitude;
            freq = SignalFreq;
            beginPhase = BeginSignalPhase;
            Length = SignalLength;
            freqNes = FreqNes;
        }

        // расчет точек несущего сигнала
        public double[] CalculatePointsNesSignal()
        {
            double[] y = new double[Length];

            for (int i = 0; i < Length; i++)
            {
                y[i] = amplitude * Math.Cos((2 * Math.PI * freqNes * i) + beginPhase);
            }

            return y;
        }

        // расчет точек информационного сигнала
        public double[] CalculatePointsInfoSignal()
        {
            double[] y = new double[Length];

            for (int i = 0; i < Length; i++)
            {
                y[i] = amplitude * Math.Cos((2 * Math.PI * freq * i) + beginPhase);
            }

            return y;
        }

        // БПФ
        Complex[] fft(Complex[] frame, bool direct) 
        {
            const double DoublePi = 2 * Math.PI;
            if (frame.Length == 1) return frame;
            int halfSampleSize = frame.Length / 2;
            int fullSampleSize = frame.Length;

            double arg = direct ? -DoublePi / fullSampleSize : DoublePi / fullSampleSize;
            Complex omegaPowBase = new Complex(Math.Cos(arg), Math.Sin(arg));
            Complex omega = Complex.One;
            Complex[] spectrum = new Complex[fullSampleSize];

            for (int j = 0; j < halfSampleSize; j++)
            {
                spectrum[j] = frame[j] + frame[j + halfSampleSize];
                spectrum[j + halfSampleSize] = omega * (frame[j] - frame[j + halfSampleSize]);
                omega *= omegaPowBase;
            }

            Complex[] yTop = new Complex[halfSampleSize];
            Complex[] yBottom = new Complex[halfSampleSize];
            for (int i = 0; i < halfSampleSize; i++)
            {
                yTop[i] = spectrum[i];
                yBottom[i] = spectrum[i + halfSampleSize];
            }

            yTop = fft(yTop, direct);
            yBottom = fft(yBottom, direct);
            for (int i = 0; i < halfSampleSize; i++)
            {
                int j = i << 1;
                spectrum[j] = yTop[i];
                spectrum[j + 1] = yBottom[i];
            }

            return spectrum;
        }

        // метод возвращает модуль комплексных значений
        double[] ComplexMagnitude(Complex[] arr)  
        {
            double[] result = new double[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                result[i] = arr[i].Magnitude;

            return result;
        }

        // метод возвращает модуль спектра
        public double[] CalculateSpectr(double[] spectr)
        {
            Complex[] spectr_c = CalculateSpectr_(spectr);
            double[] spectrum = ComplexMagnitude(spectr_c);

            return spectrum;
        }

        // метод возвращает комплексный спектр
        public Complex[] CalculateSpectr_(double[] modulated_signal)
        {
            Complex[] signal_noise_sum_c = new Complex[modulated_signal.Length];
            for (int i = 0; i < signal_noise_sum_c.Length; i++)
                signal_noise_sum_c[i] = modulated_signal[i];
            Complex[] spectrum_c = fft(signal_noise_sum_c, true);

            return spectrum_c;
        }

        // метод поиска минимума массива
        public double Min(double[] mas)
        {
            double min = mas[0];
            for (int i = 0; i < mas.Length; i++)
            {
                if (min > mas[i])
                {
                    min = mas[i];
                }
            }

            return min;
        }

        // метод, осуществляющий спуск огибающей
        public double[] OgibSpectr(double min, double[] spectr)
        {
            for (int i = 0; i < spectr.Length; i++)
            {
                spectr[i] = spectr[i] - min;
            }
                
            return spectr;
 
        }

        // возвращаем true или false
        public bool logic(List<PointD> list)
        {
            List<int> mas_check = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if ((double)(list[i].x) <= -2147483353 || (double)(list[i].x) >= 2147483353)
                {
                    mas_check.Add(1);
                }
                else
                {
                    mas_check.Add(0);
                }
            }

            bool onlyZero = mas_check.All(el => el == 0);

            return onlyZero;
        }
    }
}
