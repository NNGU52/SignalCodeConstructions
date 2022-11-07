using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCodeConstructions
{
    class MethodRecognitionModulation
    {
        int amplitude;   // амплитуда сигнала
        double freqNes;  // частота несущей
        bool b;          // проверяем чекбокс
        int M;           // кратность модуляции


        public MethodRecognitionModulation(int Amplitude, bool B, int NumberPhase)
        {
            amplitude = Amplitude;
            b = B;
            M = NumberPhase;
            freqNes = 0.9;
        }

        // метод распознавания модуляции по виду созвездия
        public List<double> SignalPhase(double[] modulated_signal, double[] nes_signal)
        {
            List<double> raznost = new List<double>();
            List<double> raznost_result = new List<double>();
            List<double> phase_m = new List<double>();
            List<double> phase_nes = new List<double>();
            List<double> buf_nes = new List<double>();

            double peremen;
            double new_value_nes;
            double start_phase;
            double phase_direct = 1;
            bool phase_correct = false;
            double porog_for_noise = 0.05;

            for (int i = 0; i < modulated_signal.Length; i++)
            {
                peremen = modulated_signal[i] / amplitude;
                phase_m.Add(Math.Acos(peremen));
            }

            start_phase = phase_m[0];

            for (int i = 0; i < modulated_signal.Length - 1; i++)
            {
                new_value_nes = amplitude * Math.Cos((2 * Math.PI * freqNes * (i)) + start_phase * phase_direct);
                buf_nes.Add(new_value_nes);
                peremen = buf_nes[i] / amplitude;
                phase_nes.Add(Math.Acos(peremen));
                double phase_raznost = Math.Round((phase_m[i] - phase_nes[i]), 6);

                if (b == false)
                {
                    if (phase_raznost != 0)
                    {
                        start_phase = phase_m[i];
                        if (phase_correct)
                        {
                            phase_direct *= -1;
                            phase_correct = false;
                        }
                        else
                        {
                            phase_correct = true;
                        }
                        new_value_nes = amplitude * Math.Cos((2 * Math.PI * freqNes * (i)) + start_phase * phase_direct);
                        buf_nes[i] = new_value_nes;
                        peremen = buf_nes[i] / amplitude;
                        phase_nes[i] = Math.Acos(peremen);
                        phase_raznost = Math.Round((phase_m[i] - phase_nes[i]), 6);
                    }
                    else
                    {
                        raznost.Add(Math.Round((start_phase * phase_direct), 6)); continue;

                    }
                }
                else
                {
                    if (Math.Abs(phase_raznost) >= porog_for_noise)
                    {
                        start_phase = phase_m[i];
                        if (phase_correct)
                        {
                            phase_direct *= -1;
                            phase_correct = false;
                        }
                        else
                        {
                            phase_correct = true;
                        }
                        new_value_nes = amplitude * Math.Cos((2 * Math.PI * freqNes * (i)) + start_phase * phase_direct);
                        buf_nes[i] = new_value_nes;
                        peremen = buf_nes[i] / amplitude;
                        phase_nes[i] = Math.Acos(peremen);
                        phase_raznost = Math.Round((phase_m[i] - phase_nes[i]), 6);
                    }
                    else
                    {
                        raznost.Add(Math.Round((start_phase * phase_direct), 6)); continue;
                    }
                }

                continue;
            }

            var h = new Dictionary<double, int>();
            foreach (double i in raznost)
            {
                int res;
                if (h.TryGetValue(i, out res))
                    h[i] += 1;
                else
                    h.Add(i, 1);
            }

            foreach (var kv in h)
                System.Diagnostics.Trace.WriteLine(kv.Key + " (" + kv.Value + ")");

            int max = 0;
            double coeff_porog = 0.1;
            double porog;
            foreach (var kv in h)
            {
                if (kv.Value > max)
                {
                    max = kv.Value;
                }
            }

            porog = coeff_porog * max;

            foreach (var kv in h)
            {
                if (kv.Value > porog)
                {
                    raznost_result.Add(kv.Key);
                }
            }

            return raznost_result;
        }

        // метод подсчета коэффициента эксцесса
        public double ValueExcess(double[] ogib_signal)
        {
            double excess;
            double sred = 0;  // находим среднее значение
            double moment_four_por = 0;
            double deviation = 0;
            double deviation_;
            for (int i = 0; i < ogib_signal.Length; i++)
            {
                sred += ogib_signal[i];
            }

            sred /= ogib_signal.Length;

            for (int i = 0; i < ogib_signal.Length; i++)
            {
                moment_four_por += Math.Pow((ogib_signal[i] - sred), 4);
                deviation += Math.Pow((ogib_signal[i] - sred), 2);
            }

            moment_four_por /= (ogib_signal.Length - 1);
            deviation /= (ogib_signal.Length - 1);
            deviation_ = Math.Pow(Math.Sqrt(deviation), 4);

            excess = (moment_four_por / deviation_) - 3;

            Console.WriteLine(excess);

            if (M == 2)
            {
                excess -= 6;
            }

            if (M == 4)
            {
                excess -= 12.4;
            }

            if (M == 8)
            {
                excess -= 16.4;
            }

            return excess;
        }
    }
}
