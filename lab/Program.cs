using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;

namespace Lab4Spline
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            double[] xNodes = {
                1.0, 1.6, 2.2, 2.9, 3.6,
                4.2, 4.8, 5.5, 6.1, 6.8,
                7.4, 8.1, 8.7, 9.3, 10.0
            };
            double[] yNodes = xNodes.Select(x => Math.Sqrt(x)).ToArray();

            Console.WriteLine("=============================================");
            Console.WriteLine("      ПРИРОДНИЙ КУБІЧНИЙ СПЛАЙН");
            Console.WriteLine("=============================================");

            Console.WriteLine("\n[1] ВХІДНІ ДАНІ:");
            Console.WriteLine($"{"i",-3} | {"x_i",-6} | {"y_i = sqrt(x)",-15}");
            Console.WriteLine(new string('-', 30));
            for (int i = 0; i < xNodes.Length; i++)
            {
                Console.WriteLine($"{i,-3} | {xNodes[i],-6:F1} | {yNodes[i],-15:F6}");
            }

            CubicSpline spline = new CubicSpline(xNodes, yNodes);

            double xTest = 5.55;
            double yExact = Math.Sqrt(xTest);
            double ySpline = spline.Interpolate(xTest);
            double d1Spline = spline.InterpolateFirstDeriv(xTest);
            double d2Spline = spline.InterpolateSecondDeriv(xTest);

            Console.WriteLine("\n[5] АНАЛІЗ У КОНТРОЛЬНІЙ ТОЧЦІ x = " + xTest);
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"Точне f(x):            {yExact:F10}");
            Console.WriteLine($"Сплайн S(x):           {ySpline:F10}");
            Console.WriteLine($"Похибка:               {Math.Abs(yExact - ySpline):E4}");
            Console.WriteLine($"Похідна S'(x):         {d1Spline:F6}");
            Console.WriteLine($"Друга похідна S''(x):  {d2Spline:F6}");

            Console.WriteLine("\n[6] ПОБУДОВА ГРАФІКІВ...");
            var plt = new Plot();
            plt.Title("Cubic Spline vs sqrt(x)");
            plt.XLabel("x"); plt.YLabel("y");

            int points = 5000;
            double[] xPlot = GenerateLinspace(xNodes[0], xNodes[xNodes.Length - 1], points);
            double[] yExactPlot = xPlot.Select(x => Math.Sqrt(x)).ToArray();
            double[] ySplinePlot = xPlot.Select(x => spline.Interpolate(x)).ToArray();

            var lineExact = plt.Add.Scatter(xPlot, yExactPlot, Colors.Orange);
            lineExact.LineWidth = 6;
            lineExact.LegendText = "Exact sqrt(x)";

            var lineSpline = plt.Add.Scatter(xPlot, ySplinePlot, Colors.Black);
            lineSpline.LineWidth = 2;
            lineSpline.LinePattern = LinePattern.Dashed;
            lineSpline.LegendText = "Spline S(x)";

            var scatter = plt.Add.Scatter(xNodes, yNodes, Colors.Red);
            scatter.LineWidth = 0;
            scatter.MarkerSize = 9;
            scatter.LegendText = "Nodes";

            plt.ShowLegend();
            plt.SavePng("plot_spline.png", 1000, 600);
            Console.WriteLine(" -> Графік збережено у 'plot_spline.png'");

            PlotDerivatives(spline, xPlot, xNodes);
        }

        static void PlotDerivatives(CubicSpline spline, double[] xPlot, double[] xNodes)
        {
            var plt1 = new Plot();
            plt1.Title("1st Derivative S'(x)");
            var d1Exact = xPlot.Select(x => 0.5 / Math.Sqrt(x)).ToArray();
            var d1Spline = xPlot.Select(x => spline.InterpolateFirstDeriv(x)).ToArray();

            plt1.Add.Scatter(xPlot, d1Exact, Colors.LightBlue).LineWidth = 6;
            plt1.Add.Scatter(xPlot, d1Spline, Colors.DarkBlue).LineWidth = 2;
            plt1.SavePng("plot_deriv1.png", 1000, 600);

            var plt2 = new Plot();
            plt2.Title("2nd Derivative S''(x)");
            var d2Exact = xPlot.Select(x => -0.25 / (x * Math.Sqrt(x))).ToArray();
            var d2Spline = xPlot.Select(x => spline.InterpolateSecondDeriv(x)).ToArray();

            plt2.Add.Scatter(xPlot, d2Exact, Colors.LightGreen).LineWidth = 6;
            plt2.Add.Scatter(xPlot, d2Spline, Colors.DarkGreen).LineWidth = 2;
            plt2.Add.Scatter(xNodes, xNodes.Select(n => spline.InterpolateSecondDeriv(n)).ToArray(), Colors.Red).MarkerSize = 5;
            plt2.SavePng("plot_deriv2.png", 1000, 600);

            Console.WriteLine(" -> Графіки похідних збережено");
        }

        static double[] GenerateLinspace(double start, double end, int count)
        {
            double[] res = new double[count];
            double step = (end - start) / (count - 1);
            for (int i = 0; i < count; i++) res[i] = start + i * step;
            return res;
        }
    }

    public class CubicSpline
    {
        private double[] x, a, b, c, d;
        private int n;

        public CubicSpline(double[] nodesX, double[] nodesY)
        {
            x = nodesX;
            a = nodesY;
            n = x.Length - 1;

            b = new double[n];
            c = new double[n + 1];
            d = new double[n];
            double[] h = new double[n];

            // 1. Обчислення кроків
            Console.WriteLine("\n[2] ОБЧИСЛЕННЯ КРОКІВ (h_i):");
            for (int i = 0; i < n; i++)
            {
                h[i] = x[i + 1] - x[i];
                Console.Write($"h_{i}={h[i]:F2}  ");
            }
            Console.WriteLine();

            // 2. Формування СЛАР для c_i
            double[] alpha = new double[n]; 

            Console.WriteLine("\n[3] ФОРМУВАННЯ СЛАР ДЛЯ КОЕФІЦІЄНТІВ c (Моментів):");
            Console.WriteLine("Рівняння: A[i]*c[i-1] + B[i]*c[i] + D[i]*c[i+1] = F[i]");
            Console.WriteLine(new string('-', 70));

            for (int i = 1; i < n; i++)
            {
                alpha[i] = 3.0 * ((a[i + 1] - a[i]) / h[i] - (a[i] - a[i - 1]) / h[i - 1]);

                double A_i = h[i - 1];
                double B_i = 2.0 * (h[i - 1] + h[i]);
                double D_i = h[i];

                Console.WriteLine($"i={i,-2}: {A_i,5:F2} * c_{i - 1,-2} + {B_i,5:F2} * c_{i,-2} + {D_i,5:F2} * c_{i + 1,-2} = {alpha[i]:F4}");
            }
            Console.WriteLine("(Граничні умови: c_0 = 0, c_n = 0)");

            // 3. Метод прогонки
            double[] l = new double[n + 1];
            double[] mu = new double[n + 1];
            double[] z = new double[n + 1];

            l[0] = 1.0; mu[0] = 0.0; z[0] = 0.0;

            for (int i = 1; i < n; i++)
            {
                l[i] = 2.0 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
                mu[i] = h[i] / l[i];
                z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
            }

            l[n] = 1.0; z[n] = 0.0; c[n] = 0.0;

            for (int j = n - 1; j >= 0; j--)
            {
                c[j] = z[j] - mu[j] * c[j + 1];
                b[j] = (a[j + 1] - a[j]) / h[j] - h[j] * (c[j + 1] + 2.0 * c[j]) / 3.0;
                d[j] = (c[j + 1] - c[j]) / (3.0 * h[j]);
            }

            // 4. Таблиця коефіцієнтів
            Console.WriteLine("\n[4] ТАБЛИЦЯ КОЕФІЦІЄНТІВ СПЛАЙНА:");
            Console.WriteLine($"{"i",-3} | {"Інтервал",-12} | {"a_i",-9} | {"b_i",-9} | {"c_i",-9} | {"d_i",-9}");
            Console.WriteLine(new string('-', 70));
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($"{i,-3} | [{x[i]:F1}; {x[i + 1]:F1}] | {a[i],-9:F4} | {b[i],-9:F4} | {c[i],-9:F4} | {d[i],-9:F4}");
            }

            // 5. Аналітичний вигляд (Нове!)
            Console.WriteLine("\n[5] АНАЛІТИЧНИЙ ВИГЛЯД СПЛАЙНА НА КОЖНОМУ ІНТЕРВАЛІ:");
            Console.WriteLine("S(x) = a + b(x-xi) + c(x-xi)^2 + d(x-xi)^3");
            Console.WriteLine(new string('-', 80));
            for (int i = 0; i < n; i++)
            {
                string s_b = b[i] >= 0 ? "+ " : "- ";
                string s_c = c[i] >= 0 ? "+ " : "- ";
                string s_d = d[i] >= 0 ? "+ " : "- ";

                string termX = $"(x - {x[i]:F1})";

                Console.WriteLine($"Інтервал [{x[i]:F1}; {x[i + 1]:F1}]:");
                Console.WriteLine($"  S_{i}(x) = {a[i]:F4} {s_b}{Math.Abs(b[i]):F4}*{termX} {s_c}{Math.Abs(c[i]):F4}*{termX}^2 {s_d}{Math.Abs(d[i]):F4}*{termX}^3");
            }
        }

        public double Interpolate(double val)
        {
            int i = FindInterval(val);
            double dx = val - x[i];
            return a[i] + b[i] * dx + c[i] * dx * dx + d[i] * dx * dx * dx;
        }

        public double InterpolateFirstDeriv(double val)
        {
            int i = FindInterval(val);
            double dx = val - x[i];
            return b[i] + 2.0 * c[i] * dx + 3.0 * d[i] * dx * dx;
        }

        public double InterpolateSecondDeriv(double val)
        {
            int i = FindInterval(val);
            double dx = val - x[i];
            return 2.0 * c[i] + 6.0 * d[i] * dx;
        }

        private int FindInterval(double val)
        {
            int i = n - 1;
            for (int k = 0; k < n; k++)
            {
                if (val >= x[k] && val < x[k + 1])
                {
                    i = k;
                    break;
                }
            }
            return i;
        }
    }
}