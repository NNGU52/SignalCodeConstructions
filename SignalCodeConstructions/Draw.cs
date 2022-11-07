using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace SignalCodeConstructions
{
    class Draw
    {
        private int Width, Height;

        // конструктор
        public Draw(int BitmapWidth, int BitmapHeight)
        {
            Width = BitmapWidth;
            Height = BitmapHeight;
        }

        // рисуем точки на окружности
        public Bitmap DrawNumberPhaseStar(List<PointD> data, int M)
        {
            Bitmap dblBuffer = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(dblBuffer);

            g.DrawEllipse(new Pen(Color.Red, 2f), 150, 50, 300, 300);

            for (int i = 0; i < M; i++)
            {
                g.FillEllipse(new SolidBrush(Color.Black), data[i].x, data[i].y, 10, 10);
            }

            return dblBuffer;
        }

        // рисуем точки на окружности (восстановленное созвездие)
        public Bitmap DrawNumberPhaseStar_recovery(List<PointD> data)
        {
            Bitmap dblBuffer = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(dblBuffer);

            g.DrawEllipse(new Pen(Color.Red, 2f), 150, 50, 300, 300);

            for (int i = 0; i < data.Count; i++)
            {
                g.FillEllipse(new SolidBrush(Color.Black), data[i].x, data[i].y, 10, 10);
            }

            return dblBuffer;
        }

        // рисуем графики
        public void SendChartNoModulated(Chart ChartI, double[] y, int length)
        {
            ChartI.Series[0].Points.Clear();

            for (int i = 0; i < length; i++)
            {
                ChartI.Series[0].Points.AddXY(i, y[i]);
            }
        }

        // отрисовка уровней кванования
        public void SendChartValueUroven(Chart ChartI, double[] y, int length)
        {
            ChartI.Series[1].Points.Clear();

            for (int k = 0; k < y.Length; k++)
            {
                for (int i = 0; i < length; i++)
                {
                    ChartI.Series[1].Points.AddXY(i, y[k]);
                }
            }
        }

        // метод отрисовки графика зависимости
        public void SendChartList(Chart ChartI, List<double> snr, List<double> excess, List<double> snr1, List<double> excess1, List<double> snr2, List<double> excess2)
        {
            ChartI.Series[0].Points.Clear();
            ChartI.Series[1].Points.Clear();
            ChartI.Series[2].Points.Clear();

            for (int k = 0; k < snr.Count; k++)
            {
                ChartI.Series[0].Points.AddXY(snr[k], excess[k]);
            }

            for (int k = 0; k < snr1.Count; k++)
            {
                ChartI.Series[1].Points.AddXY(snr1[k], excess1[k]);
            }

            for (int k = 0; k < snr2.Count; k++)
            {
                ChartI.Series[2].Points.AddXY(snr2[k], excess2[k]);
            }
        }
    }
}
