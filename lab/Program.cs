using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;

namespace Lab4;

class Program
{
    static double F(double x) => Math.Sqrt(x);

    static double GetDerivative(int n, double x)
    {
        if (n == 0) return Math.Sqrt(x);

        double coeff = 1.0;
        for (int k = 0; k < n; k++)
        {
            coeff *= (0.5 - k);
        }
        return coeff * Math.Pow(x, 0.5 - n);
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        double a = 1.0;
        double b = 10.0;

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

        //МЕТОД ЛАГРАНЖА
        PrintHeader("1. МЕТОД ЛАГРАНЖА");

        Console.WriteLine("Аналітичний розв'язок (значення у вузлах):");

        for (int i = 0; i < xNodesLag.Length; i++)
        {
            Console.WriteLine($"x_{i} = {xNodesLag[i]:F1} => f(x_{i}) = sqrt({xNodesLag[i]:F1}) = {yNodesLag[i]:F6}");
        }
        Console.WriteLine();

        Console.WriteLine($"Таблиця вузлів ({xNodesLag.Length} точок):");
        Console.WriteLine($"{"i",-5} | {"x_i",-10} | {"f(x_i)",-15}");
        Console.WriteLine(new string('-', 35));
        for (int i = 0; i < xNodesLag.Length; i++)
        {
            Console.WriteLine($"{i,-5} | {xNodesLag[i],-10:F4} | {yNodesLag[i],-15:F6}");
        }
        Console.WriteLine($"\nПоліном 14-го степеня.");


        PrintHeader("2. МЕТОД ЕРМІТА (З ТРИКУТНОЮ ТАБЛИЦЕЮ)");

        var (hZ, hCoeffs) = BuildHermiteTableVerbose(xHermiteBase, hMultiplicities);

        Console.WriteLine("\nФормула полінома Ерміта:");
        Console.Write($"H(x) = {hCoeffs[0]:F4}");
        for (int i = 1; i < Math.Min(5, hCoeffs.Length); i++)
            Console.Write($" + ({hCoeffs[i]:F4}) * product...");
        Console.WriteLine(" + ...");

        int plotPoints = 500;
        double[] xPlot = GenerateLinspace(a, b, plotPoints);

        double[] yExact = new double[plotPoints];
        double[] yLagrange = new double[plotPoints];
        double[] yHermite = new double[plotPoints];

        for (int i = 0; i < plotPoints; i++)
        {
            double xi = xPlot[i];
            yExact[i] = F(xi);
            yLagrange[i] = LagrangeManual(xi, xNodesLag, yNodesLag);
            yHermite[i] = EvalHermite(xi, hZ, hCoeffs);
        }

        double xTest = 5.55;
        double valExact = F(xTest);
        double valLag = LagrangeManual(xTest, xNodesLag, yNodesLag);
        double valHer = EvalHermite(xTest, hZ, hCoeffs);

        PrintHeader("АНАЛІЗ РЕЗУЛЬТАТІВ");
        Console.WriteLine($"Контрольна точка x = {xTest}");
        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"{"МЕТОД",-15} | {"ЗНАЧЕННЯ P(x)",-20} | {"ПОХИБКА |f-P|",-20}");
        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"{"Точне f(x)",-15} | {valExact,-20:F10} | {"-",-20}");
        Console.WriteLine($"{"Лагранж",-15} | {valLag,-20:F10} | {Math.Abs(valExact - valLag),-20:E4}");
        Console.WriteLine($"{"Ерміт",-15} | {valHer,-20:F10} | {Math.Abs(valExact - valHer),-20:E4}");
        Console.WriteLine(new string('-', 75));


        //Лагранж
        var plt1 = new Plot();
        plt1.Title("Метод Лагранжа (15 точок)");
        plt1.XLabel("X"); plt1.YLabel("Y");

        var lineEx1 = plt1.Add.Scatter(xPlot, yExact, Colors.Orange);
        lineEx1.LegendText = "Exact f(x)";
        lineEx1.LinePattern = LinePattern.Dashed;
        lineEx1.LineWidth = 2;

        var lineLag1 = plt1.Add.Scatter(xPlot, yLagrange, Colors.Black);
        lineLag1.LinePattern = LinePattern.Dotted;
        lineLag1.LegendText = "Lagrange";
        lineLag1.LineWidth = 2;

        var scatL = plt1.Add.Scatter(xNodesLag, yNodesLag, Colors.Red);
        scatL.LineWidth = 0; scatL.MarkerSize = 9; scatL.LegendText = "Nodes";

        plt1.ShowLegend();
        plt1.SavePng("plot_lagrange.png", 1000, 600);

        // Ерміт
        var plt2 = new Plot();
        plt2.Title("Метод Ерміта (35 умов)");
        plt2.XLabel("X"); plt2.YLabel("Y");

        var lineEx2 = plt2.Add.Scatter(xPlot, yExact, Colors.Orange);
        lineEx2.LegendText = "Exact f(x)";
        lineEx2.LinePattern = LinePattern.Dashed;
        lineEx2.LineWidth = 2;

        var lineHer2 = plt2.Add.Scatter(xPlot, yHermite, Colors.Blue);
        lineHer2.LegendText = "Hermite";
        lineHer2.LinePattern = LinePattern.Dotted;
        lineHer2.LineWidth = 2;

        for (int i = 0; i < xHermiteBase.Length; i++)
        {
            double x = xHermiteBase[i];
            double y = F(x);
            int m = hMultiplicities[i];
            for (int k = 0; k < m; k++)
            {
                float size = 14 - (k * 3); if (size < 3) size = 3;
                var mk = plt2.Add.Marker(x, y);
                mk.Shape = MarkerShape.OpenCircle; mk.Color = Colors.DarkGreen; mk.Size = size;
                if (k == m - 1) { mk.Shape = MarkerShape.FilledCircle; mk.Color = Colors.Lime; }
            }
        }
        plt2.ShowLegend();
        plt2.SavePng("plot_hermite.png", 1000, 600);
    }

    static double LagrangeManual(double x, double[] nodes, double[] vals)
    {
        double res = 0; int n = nodes.Length;
        for (int i = 0; i < n; i++)
        {
            double l = 1;
            for (int j = 0; j < n; j++) if (i != j) l *= (x - nodes[j]) / (nodes[i] - nodes[j]);
            res += vals[i] * l;
        }
        return res;
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

        int rL = N;
        int cL = N;

        Console.Write($"{"z_i",-6} | {"f(z)",-8} | ");
        for (int j = 1; j < cL; j++) Console.Write($"{"O" + j,-8} | ");
        Console.WriteLine("\n" + new string('-', 10 + cL * 11));

        for (int i = 0; i < rL; i++)
        {
            Console.Write($"{z[i],-6:F1} | {table[i, 0],-8:F3} | ");
            for (int j = 1; j < cL && j < N - i; j++)
            {
                double val = table[i, j];
                string sVal = (Math.Abs(val) > 1000 || (Math.Abs(val) < 0.001 && val != 0))
                    ? $"{val:E1}"
                    : $"{val:F3}";

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