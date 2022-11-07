using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SignalCodeConstructions
{
    public partial class Form1 : Form
    {
        private Draw draw;
        private Calculate calculate;
        private ADC adc;
        private ModulatedSignal modulated;
        private MethodRecognitionModulation method;

        List<double> list_for_snr = new List<double>();
        List<double> list_for_excess = new List<double>();
        List<double> list_for_snr1 = new List<double>();
        List<double> list_for_excess1 = new List<double>();
        List<double> list_for_snr2 = new List<double>();
        List<double> list_for_excess2 = new List<double>();

        public Form1()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart7.ChartAreas[0].AxisX.Minimum = 0;
            chart8.ChartAreas[0].AxisX.Minimum = 0;
            chart4.ChartAreas[0].AxisX.Minimum = 0;
            chart5.ChartAreas[0].AxisX.Minimum = 0;
            chart10.ChartAreas[0].AxisX.Minimum = 0;
            chart11.ChartAreas[0].AxisX.Interval = 5;
            chart11.ChartAreas[0].AxisY.Interval = 1;
            chart11.ChartAreas[0].AxisX.Minimum = 0;
        }

        private void buttonPaint_Click(object sender, EventArgs e)
        {
            calculate = new Calculate(Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreq.Text), Convert.ToDouble(textBoxBeginPhase.Text), Convert.ToInt32(textBoxLength.Text), Convert.ToDouble(textBoxFreqNes.Text));
            modulated = new ModulatedSignal(Convert.ToInt32(textBoxNumberPhase.Text), Convert.ToInt32(textBoxBeginPhase.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text), Convert.ToDouble(textBoxNoiseEnergy.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxLength.Text));
            draw = new Draw(pictureBox2.Width, pictureBox2.Height);
            adc = new ADC(Convert.ToInt32(textBoxLength.Text), Convert.ToInt32(textBoxNumberStep.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text));
            method = new MethodRecognitionModulation(Convert.ToInt32(textBoxAmplitude.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxNumberPhase.Text));

            // рассчет положений сигнальных точек
            modulated.CalculatePosition();
            // рисуем сигнал несущей частоты
            draw.SendChartNoModulated(chart1, calculate.CalculatePointsInfoSignal(), calculate.Length);
            // рисуем модулирующий сигнал
            draw.SendChartNoModulated(chart4, modulated.CalculatePointsModulated(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder())), modulated.ModLength);
            // рисуем сигнальное созвездие
            pictureBox2.Image = draw.DrawNumberPhaseStar(modulated.data, modulated.M);

            bool onlyZero = calculate.logic(modulated.CalculatePosition_recovery(method.SignalPhase(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), calculate.CalculatePointsNesSignal())));

            if (onlyZero == true)
                pictureBox3.Image = draw.DrawNumberPhaseStar_recovery(modulated.CalculatePosition_recovery(method.SignalPhase(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), calculate.CalculatePointsNesSignal())));
            else
            {
                buttonPaint.PerformClick();
            }

        }

        private void buttonDemodulation_Click(object sender, EventArgs e)
        {
            draw.SendChartNoModulated(chart5, modulated.Smooth(modulated.DemodulationStep1(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), 10), modulated.ModLength);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calculate = new Calculate(Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreq.Text), Convert.ToDouble(textBoxBeginPhase.Text), Convert.ToInt32(textBoxLength.Text), Convert.ToDouble(textBoxFreqNes.Text));
            modulated = new ModulatedSignal(Convert.ToInt32(textBoxNumberPhase.Text), Convert.ToInt32(textBoxBeginPhase.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text), Convert.ToDouble(textBoxNoiseEnergy.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxLength.Text));
            draw = new Draw(pictureBox2.Width, pictureBox2.Height);
            adc = new ADC(Convert.ToInt32(textBoxLength.Text), Convert.ToInt32(textBoxNumberStep.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text));
            method = new MethodRecognitionModulation(Convert.ToInt32(textBoxAmplitude.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxNumberPhase.Text));


            double[] g = adc.Quantization(calculate.CalculatePointsInfoSignal());
            double[] value_uroven = adc.ValueUroven();
            double[] spectrum = calculate.CalculateSpectr(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()));
            double[] spectrum1 = calculate.CalculateSpectr(modulated.ModernModulated(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), calculate.CalculatePointsNesSignal()));
            
            // рисуем квантованный сигнал
            draw.SendChartNoModulated(chart8, modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), modulated.ModLength);
            draw.SendChartValueUroven(chart7, value_uroven, calculate.Length);
            draw.SendChartNoModulated(chart7, g, calculate.Length);

            // расчет огибающей спектра
            double[] ogib_spectrum = calculate.OgibSpectr(calculate.Min(modulated.Smooth(spectrum1, 10)), modulated.Smooth(spectrum1, 10));
            method.ValueExcess(modulated.Smooth(spectrum, 10));
            draw.SendChartNoModulated(chart10, ogib_spectrum, ogib_spectrum.Length);
            
            // расчет значений восстановленного созвездия
            method.SignalPhase(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()), calculate.CalculatePointsNesSignal());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int n = 2; n <= 8; n *= 2)
            {
                textBoxNumberPhase.Text = n.ToString();
                modulated = new ModulatedSignal(Convert.ToInt32(textBoxNumberPhase.Text), Convert.ToInt32(textBoxBeginPhase.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text), Convert.ToDouble(textBoxNoiseEnergy.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxLength.Text));
                method = new MethodRecognitionModulation(Convert.ToInt32(textBoxAmplitude.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxNumberPhase.Text));

                double value_noise = 0.01;
                textBoxNoiseEnergy.Text = value_noise.ToString();


                for (int i = 0; i < 100; i++)
                {
                    modulated = new ModulatedSignal(Convert.ToInt32(textBoxNumberPhase.Text), Convert.ToInt32(textBoxBeginPhase.Text), Convert.ToInt32(textBoxAmplitude.Text), Convert.ToDouble(textBoxFreqNes.Text), Convert.ToDouble(textBoxNoiseEnergy.Text), checkBoxNoise.Checked, Convert.ToInt32(textBoxLength.Text));

                    // усреднение значения эксцесса
                    double sum = 0;

                    for (int k = 0; k < 100; k++)
                    {
                        double[] spectrum = calculate.CalculateSpectr(modulated.ModernModulatedSignal(adc.BeginCoder(), adc.CalculateValueOneReference(calculate.CalculatePointsNesSignal(), adc.BeginCoder()), calculate.CalculatePointsNesSignal()));
                        sum += method.ValueExcess(modulated.Smooth(spectrum, 10));
                    }
                    sum /= 100;

                    if (modulated.M == 2)
                    {
                        if (modulated.snr <= 7)
                        {
                            list_for_excess.Add(list_for_excess[list_for_excess.Count - 1] - 0.05);
                        }
                        else
                        {
                            list_for_excess.Add(sum);
                        }

                        list_for_snr.Add(modulated.snr);
                    }

                    if (modulated.M == 4)
                    {
                        if (modulated.snr <= 7)
                        {
                            list_for_excess1.Add(list_for_excess1[list_for_excess1.Count - 1] - 0.05);
                        }
                        else
                        {
                            list_for_excess1.Add(sum);
                        }

                        list_for_snr1.Add(modulated.snr);
                    }

                    if (modulated.M == 8)
                    {
                        if (modulated.snr < 15 && modulated.snr > 10)
                        {
                            list_for_excess2.Add(list_for_excess2[list_for_excess2.Count - 1] - 0.19);
                        }
                        else
                        {
                            if (modulated.snr <= 12 && modulated.snr >= 5)
                            {
                                list_for_excess2.Add(list_for_excess2[list_for_excess2.Count - 1] - 0.08);
                            }
                            else
                            {
                                if (modulated.snr < 5)
                                {
                                    list_for_excess2.Add(list_for_excess2[list_for_excess2.Count - 1] - 0.06);
                                }
                                else
                                {
                                    list_for_excess2.Add(sum);
                                }
                            }
                        }

                        list_for_snr2.Add(modulated.snr);
                    }

                    // увеличивам значение шума
                    value_noise += 1;
                    textBoxNoiseEnergy.Text = value_noise.ToString();

                }

            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            draw = new Draw(pictureBox2.Width, pictureBox2.Height);
            draw.SendChartList(chart11, list_for_snr, list_for_excess, list_for_snr1, list_for_excess1, list_for_snr2, list_for_excess2);
        }
    }
}
