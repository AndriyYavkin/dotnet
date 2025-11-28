using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;

namespace Lab4
{
    class Program
    {
        static double F(double x) => Math.Sqrt(x);

        static double GetDerivative(int n, double x)
        {
            if (n == 0) return Math.Sqrt(x);
            double coeff = 1.0;
            for (int k = 0; k < n; k++) coeff *= (0.5 - k);
            return coeff * Math.Pow(x, 0.5 - n);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            double a = 1.0;
            double b = 10.0;

            double xTest = 5.55;
            double valExact = F(xTest);

            double[] xNodesLag = new double[]
            {
                1.0, 1.6, 2.2, 2.9, 3.6,
                4.2, 4.8, 5.5, 6.1, 6.8,
                7.4, 8.1, 8.7, 9.3, 10.0
            };
            double[] yNodesLag = xNodesLag.Select(F).ToArray();

            double[] xHermiteBase = new double[]
            {
                1.0, 2.3, 3.6, 4.9,
                6.1, 7.4, 8.7, 10.0
            };
            int[] hMultiplicities = { 4, 4, 4, 4, 4, 4, 4, 1 };

            // МЕТОД ЛАГРАНЖА
            PrintHeader("1. МЕТОД ЛАГРАНЖА");

            Console.WriteLine("Аналітичний розв'язок (перевірка у вузлах):");
            for (int i = 0; i < xNodesLag.Length; i++)
            {
                Console.WriteLine($"x_{i} = {xNodesLag[i]:F1} => f(x_{i}) = {yNodesLag[i]:F6}");
            }
            Console.WriteLine();
            Console.WriteLine("Поліном 14-го степеня.");

            Console.WriteLine("Формула: P(x) = SUM( y_i * l_i(x) )");

            double valLag = Calculate_Px_Lagrange_Verbose(xTest, xNodesLag, yNodesLag);

            Console.WriteLine($"\n>> Результат P({xTest}) = {valLag:F10}");
            Console.WriteLine($">> Точне значення:      {valExact:F10}");
            Console.WriteLine($">> Похибка:             {Math.Abs(valExact - valLag):E4}");


            //МЕТОД ЕРМІТА
            PrintHeader("2. МЕТОД ЕРМІТА");

            Console.WriteLine("Попередній розрахунок похідних (для таблиці):");
            Console.WriteLine($"{"x_i",-6} | {"f'(x)",-10} | {"f''(x)",-10} | {"f'''(x)",-10}");
            Console.WriteLine(new string('-', 45));
            for (int i = 0; i < xHermiteBase.Length - 1; i++)
            {
                double x = xHermiteBase[i];
                double d1 = GetDerivative(1, x);
                double d2 = GetDerivative(2, x);
                double d3 = GetDerivative(3, x);
                Console.WriteLine($"{x,-6:F1} | {d1,-10:F4} | {d2,-10:F4} | {d3,-10:F4}");
            }
            Console.WriteLine("(x=10.0 має лише значення f(x))\n");

            var (hZ, hCoeffs) = BuildHermiteTableVerbose(xHermiteBase, hMultiplicities);

            Console.WriteLine("Використовуємо схему Горнера з коефіцієнтами таблиці:");

            double valHer = EvalHermite_Verbose(xTest, hZ, hCoeffs);

            Console.WriteLine($"\n>> Результат H({xTest}) = {valHer:F10}");
            Console.WriteLine($">> Точне значення:      {valExact:F10}");
            Console.WriteLine($">> Похибка:             {Math.Abs(valExact - valHer):E4}");


            // ГРАФІКИ
            Console.WriteLine("\nПобудова графіків...");

            int plotPoints = 5000;
            double[] xPlot = GenerateLinspace(a, b, plotPoints);
            double[] yExact = new double[plotPoints];
            double[] yLagrange = new double[plotPoints];
            double[] yHermite = new double[plotPoints];

            for (int i = 0; i < plotPoints; i++)
            {
                double xi = xPlot[i];
                yExact[i] = F(xi);
                yLagrange[i] = Calculate_Px_Lagrange(xi, xNodesLag, yNodesLag);
                yHermite[i] = EvalHermite(xi, hZ, hCoeffs);
            }

            // Лагранж
            var plt1 = new Plot();
            plt1.Title("Метод Лагранжа (15 точок)");
            plt1.XLabel("X"); plt1.YLabel("Y");

            var lineEx1 = plt1.Add.Scatter(xPlot, yExact, Colors.Orange);
            lineEx1.LegendText = "Exact f(x)";
            lineEx1.LinePattern = LinePattern.Solid;
            lineEx1.LineWidth = 4;

            var lineLag1 = plt1.Add.Scatter(xPlot, yLagrange, Colors.Black);
            lineLag1.LegendText = "Lagrange";
            lineLag1.LinePattern = LinePattern.Dashed;
            lineLag1.LineWidth = 2;

            var scatL = plt1.Add.Scatter(xNodesLag, yNodesLag, Colors.Red);
            scatL.LineWidth = 0; scatL.MarkerSize = 9; scatL.LegendText = "Nodes";

            plt1.ShowLegend();
            plt1.SavePng("plot_lagrange.png", 1000, 600);
            Console.WriteLine(" -> plot_lagrange.png");

            // Ерміт
            var plt2 = new Plot();
            plt2.Title("Метод Ерміта (35 умов)");
            plt2.XLabel("X"); plt2.YLabel("Y");

            var lineEx2 = plt2.Add.Scatter(xPlot, yExact, Colors.Orange);
            lineEx2.LegendText = "Exact f(x)";
            lineEx2.LinePattern = LinePattern.Solid;
            lineEx2.LineWidth = 4;

            var lineHer2 = plt2.Add.Scatter(xPlot, yHermite, Colors.Blue);
            lineHer2.LegendText = "Hermite";
            lineHer2.LinePattern = LinePattern.Dashed;
            lineHer2.LineWidth = 2;

            for (int i = 0; i < xHermiteBase.Length; i++)
            {
                double x = xHermiteBase[i];
                double y = F(x);
                int m = hMultiplicities[i];

                for (int k = 0; k < m; k++)
                {
                    float size = 22 - (k * 5);
                    if (size < 4) size = 4;

                    var mk = plt2.Add.Marker(x, y);
                    mk.Shape = MarkerShape.OpenCircle;
                    mk.Color = Colors.DarkGreen;
                    mk.Size = size;
                    mk.LineWidth = 1.5f;
                }
            }
            var dummyNode = plt2.Add.Marker(0, 0);
            dummyNode.Color = Colors.Transparent;
            dummyNode.LegendText = "Nodes (Кільця = Кратність)";

            plt2.ShowLegend();
            plt2.SavePng("plot_hermite.png", 1000, 600);
            Console.WriteLine(" -> plot_hermite.png");
        }

        static double Calculate_Px_Lagrange(double x, double[] nodes, double[] values)
        {
            double Px = 0;
            int n = nodes.Length;
            for (int i = 0; i < n; i++)
            {
                double l_i = 1.0;
                for (int j = 0; j < n; j++)
                    if (i != j) l_i *= (x - nodes[j]) / (nodes[i] - nodes[j]);
                Px += values[i] * l_i;
            }
            return Px;
        }

        static double Calculate_Px_Lagrange_Verbose(double x, double[] nodes, double[] values)
        {
            double Px = 0;
            int n = nodes.Length;
            Console.WriteLine($"{"i",-3} | {"x_i",-6} | {"y_i",-10} | {"l_i(x)",-12} | {"term_i (y*l)",-12}");
            Console.WriteLine(new string('-', 60));

            for (int i = 0; i < n; i++)
            {
                double l_i = 1.0;
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        l_i *= (x - nodes[j]) / (nodes[i] - nodes[j]);
                }
                double term = values[i] * l_i;
                Px += term;

                Console.WriteLine($"{i,-3} | {nodes[i],-6:F1} | {values[i],-10:F4} | {l_i,-12:F4} | {term,-12:F6}");
            }
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"СУМА (P(x)): {Px:F10}");
            return Px;
        }

        static (double[], double[]) BuildHermiteTableVerbose(double[] nodes, int[] mults)
        {
            List<double> zList = new List<double>();
            for (int i = 0; i < nodes.Length; i++)
                for (int k = 0; k < mults[i]; k++) zList.Add(nodes[i]);
            double[] z = zList.ToArray();
            int N = z.Length;
            double[,] table = new double[N, N];

            for (int i = 0; i < N; i++) table[i, 0] = F(z[i]);

            for (int j = 1; j < N; j++)
            {
                for (int i = 0; i < N - j; i++)
                {
                    if (Math.Abs(z[i] - z[i + j]) < 1e-9)
                        table[i, j] = GetDerivative(j, z[i]) / Factorial(j);
                    else
                        table[i, j] = (table[i + 1, j - 1] - table[i, j - 1]) / (z[i + j] - z[i]);
                }
            }

            Console.WriteLine("\n>>> ТАБЛИЦЯ РОЗДІЛЕНИХ РІЗНИЦЬ <<<");
            int rL = N; int cL = N;
            Console.Write($"{"z_i",-6} | {"f(z)",-8} | ");
            for (int j = 1; j < cL; j++) Console.Write($"{"dx " + j,-8} | ");
            Console.WriteLine("\n" + new string('-', 10 + cL * 11));
            for (int i = 0; i < rL; i++)
            {
                Console.Write($"{z[i],-6:F1} | {table[i, 0],-8:F3} | ");
                for (int j = 1; j < cL && j < N - i; j++)
                {
                    double val = table[i, j];
                    string sVal = (Math.Abs(val) > 1000 || (Math.Abs(val) < 0.001 && val != 0))
                        ? $"{val:E1}" : $"{val:F3}";
                    Console.Write($"{sVal,-8} | ");
                }
                Console.WriteLine();
            }

            double[] c = new double[N];
            for (int i = 0; i < N; i++) c[i] = table[0, i];
            return (z, c);
        }

        static double EvalHermite(double x, double[] z, double[] c)
        {
            double res = c[c.Length - 1];
            for (int i = c.Length - 2; i >= 0; i--) res = res * (x - z[i]) + c[i];
            return res;
        }

        static double EvalHermite_Verbose(double x, double[] z, double[] c)
        {
            int n = c.Length;
            double res = c[n - 1];

            Console.WriteLine($"Початкове значення (останній коеф.): {res:F5}");

            for (int i = n - 2; i >= 0; i--)
            {
                double oldRes = res;
                res = res * (x - z[i]) + c[i];

                Console.WriteLine($"Крок {n - 1 - i,2}: ({oldRes,10:F5} * ({x:F2} - {z[i]:F2})) + {c[i],10:F5} = {res,10:F5}");
            }
            return res;
        }

        static double Factorial(int n)
        {
            double res = 1;
            for (int i = 2; i <= n; i++) res *= i;
            return res;
        }

        static double[] GenerateLinspace(double start, double end, int count)
        {
            double[] res = new double[count];
            double step = (end - start) / (count - 1);
            for (int i = 0; i < count; i++) res[i] = start + i * step;
            return res;
        }

        static void PrintHeader(string t) => Console.WriteLine($"\n=== {t} ===");
    }
}