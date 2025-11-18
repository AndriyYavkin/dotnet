using System;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        double[,] A = {
            { 15,  2,  1,  1,  3 },
            {  2, 18,  2, -1,  1 },
            {  1,  2, 20,  2,  1 },
            {  1, -1,  2, 16,  2 },
            {  3,  1,  1,  2, 14 }
        };

        int n = 5;
        double eps = 0.01;

        double[,] V = new double[n, n];
        for (int i = 0; i < n; i++) V[i, i] = 1.0;

        Console.WriteLine("=== Вхідна матриця (5x5) ===");
        PrintMatrix(A, n);
        Console.WriteLine($"\nПеревірка симетрії: {(IsSymmetric(A, n) ? "Так" : "Ні")}");
        Console.WriteLine($"Точність (epsilon): {eps}");
        Console.WriteLine("\n=== Початок методу обертання Якобі ===\n");

        int iteration = 0;
        while (true)
        {
            iteration++;

            double maxVal = 0.0;
            int p = -1, q = -1;
            double sumSqNonDiag = 0.0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        sumSqNonDiag += A[i, j] * A[i, j];

                        if (i < j && Math.Abs(A[i, j]) > maxVal)
                        {
                            maxVal = Math.Abs(A[i, j]);
                            p = i;
                            q = j;
                        }
                    }
                }
            }

            if (maxVal < eps)
            {
                Console.WriteLine($"\nКритерій зупинки досягнуто: макс. недіаг. елемент {maxVal:F5} < {eps}");
                break;
            }

            double App = A[p, p];
            double Aqq = A[q, q];
            double Apq = A[p, q];

            double theta = (Aqq - App) / (2 * Apq);
            double t;

            if (theta >= 0)
                t = 1.0 / (theta + Math.Sqrt(theta * theta + 1));
            else
                t = -1.0 / (-theta + Math.Sqrt(theta * theta + 1));

            double c = 1.0 / Math.Sqrt(1 + t * t);
            double s = c * t;

            double phiRad = Math.Atan(t);
            double phiDeg = phiRad * (180 / Math.PI);

            Console.WriteLine($"--- Ітерація {iteration} ---");
            Console.WriteLine($"Макс. елемент для обнулення: A[{p},{q}] = {Apq:F5}");
            Console.WriteLine($"Сума квадратів недіаг. ел.: {sumSqNonDiag:F5}");
            Console.WriteLine($"Кут повороту (phi): {phiRad:F5} рад ({phiDeg:F2}°)");

            Console.WriteLine("Матриця обертання U:");
            double[,] RotationU = new double[n, n];
            for (int i = 0; i < n; i++) RotationU[i, i] = 1.0;

            RotationU[p, p] = c;
            RotationU[q, q] = c;
            RotationU[p, q] = -s;
            RotationU[q, p] = s;

            PrintMatrix(RotationU, n);

            double prevApp = A[p, p];
            double prevAqq = A[q, q];

            A[p, p] = c * c * prevApp - 2 * s * c * Apq + s * s * prevAqq;
            A[q, q] = s * s * prevApp + 2 * s * c * Apq + c * c * prevAqq;
            A[p, q] = 0.0;
            A[q, p] = 0.0;

            for (int i = 0; i < n; i++)
            {
                if (i != p && i != q)
                {
                    double Api = A[p, i];
                    double Aqi = A[q, i];

                    A[p, i] = c * Api - s * Aqi;
                    A[i, p] = A[p, i];

                    A[q, i] = s * Api + c * Aqi;
                    A[i, q] = A[q, i];
                }
            }

            for (int i = 0; i < n; i++)
            {
                double Vip = V[i, p];
                double Viq = V[i, q];

                V[i, p] = c * Vip - s * Viq;
                V[i, q] = s * Vip + c * Viq;
            }

            Console.WriteLine("Матриця A після обертання:");
            PrintMatrix(A, n);
            Console.WriteLine();
        }

        Console.WriteLine($"\nМетод зійшовся за {iteration} ітерацій (але цикл зупинився перед {iteration + 1}).");

        Console.WriteLine("\n=== ФІНАЛЬНІ РЕЗУЛЬТАТИ ===");
        Console.WriteLine("Власні числа (діагональ матриці A):");
        Console.Write("[ ");
        for (int i = 0; i < n; i++)
        {
            Console.Write($"{A[i, i]:F5} ");
        }
        Console.WriteLine("]");

        Console.WriteLine("\nМатриця власних векторів (V):");
        PrintMatrix(V, n);

        Console.ReadKey();
    }

    static void PrintMatrix(double[,] matrix, int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double val = matrix[i, j];
                if (Math.Abs(val) < 0.00001) val = 0.0;
                Console.Write($"{val,10:F4} ");
            }
            Console.WriteLine();
        }
    }

    static bool IsSymmetric(double[,] matrix, int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (Math.Abs(matrix[i, j] - matrix[j, i]) > 0.0000001)
                    return false;
            }
        }
        return true;
    }
}