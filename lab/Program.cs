using System;

namespace NumericalMethodsLab1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // --- 1. Вхідні дані ---
            double[,] A = {
                { 6,  1,  1, -1 },
                { 1,  8,  1,  2 },
                { 1,  1,  7,  1 },
                { -1, 2,  1,  9 }
            };
            
            double[] b = { 7, 12, 10, 11 };

            Console.WriteLine("=== Вхідна система ===");
            PrintSystem(A, b);
            Console.WriteLine("Перевірка симетрії: " + (MatrixChecker.IsSymmetric(A) ? "Так" : "Ні"));
            Console.WriteLine("Перевірка діагональної переваги: " + (MatrixChecker.IsDiagonallyDominant(A) ? "Так" : "Ні"));
            Console.WriteLine();

            // 2. Метод Гауса
            Console.WriteLine("======================================");
            Console.WriteLine("   МЕТОД ГАУСА (по рядкам)");
            Console.WriteLine("======================================");
            try
            {
                var gaussResult = GaussianSolver.Solve(A.Clone() as double[,], b.Clone() as double[]);
                Console.WriteLine("\n>> Результат (Гаус):");
                PrintVector(gaussResult);
            }
            catch (Exception ex) { Console.WriteLine($"Помилка: {ex.Message}"); }

            // 3. Метод Квадратного Кореня
            Console.WriteLine("\n=================================");
            Console.WriteLine("   МЕТОД КВАДРАТНОГО КОРЕНЯ ");
            Console.WriteLine("=================================");
            try
            {
                var sqRootResult = SquareRootSolver.Solve(A.Clone() as double[,], b.Clone() as double[]);
                Console.WriteLine("\n>> Результат (Кв. корінь):");
                PrintVector(sqRootResult);
            }
            catch (Exception ex) { Console.WriteLine($"Помилка: {ex.Message}"); }

            //4. Метод Якобі
            Console.WriteLine("\n======================================");
            Console.WriteLine("   МЕТОД ЯКОБІ");
            Console.WriteLine("======================================");
            try
            {
                Console.Write("Введіть точність (за замовчуванням 0.0001): ");
                string input = Console.ReadLine();
                double epsilon = string.IsNullOrEmpty(input) ? 0.0001 : double.Parse(input.Replace('.', ','));

                var jacobiResult = JacobiSolver.Solve(A, b, epsilon);
                Console.WriteLine($"\n>> Результат (Якобі) за {jacobiResult.Iterations} ітерацій:");
                PrintVector(jacobiResult.Solution);
            }
            catch (Exception ex) { Console.WriteLine($"Помилка: {ex.Message}"); }

            Console.ReadKey();
        }

        static void PrintSystem(double[,] A, double[] b)
        {
            int n = b.Length;
            for (int i = 0; i < n; i++)
            {
                Console.Write("| ");
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{A[i, j],5:0.##} ");
                }
                Console.WriteLine($"| {b[i],5:0.##} |");
            }
        }

        static void PrintMatrix(double[,] M, string name)
        {
            int n = M.GetLength(0);
            Console.WriteLine($"Матриця {name}:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{M[i, j],8:F4} ");
                }
                Console.WriteLine();
            }
        }

        static void PrintVector(double[] x)
        {
            for (int i = 0; i < x.Length; i++)
            {
                Console.Write($"x{i + 1}={x[i]:F5}; ");
            }
            Console.WriteLine();
        }
    }

    public static class MatrixChecker
    {
        public static bool IsDiagonallyDominant(double[,] A)
        {
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++) if (i != j) sum += Math.Abs(A[i, j]);
                if (Math.Abs(A[i, i]) <= sum) return false;
            }
            return true;
        }
        public static bool IsSymmetric(double[,] A)
        {
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (Math.Abs(A[i, j] - A[j, i]) > 1e-9) return false;
            return true;
        }
    }

    // Метод Гауса
    public static class GaussianSolver
    {
        public static double[] Solve(double[,] A, double[] b)
        {
            int n = b.Length;
            
            for (int k = 0; k < n - 1; k++)
            {
                Console.WriteLine($"--- Крок прямого ходу {k + 1} ---");
                for (int i = k + 1; i < n; i++)
                {
                    double factor = A[i, k] / A[k, k];
                    b[i] -= factor * b[k];
                    for (int j = k; j < n; j++)
                    {
                        A[i, j] -= factor * A[k, j];
                    }
                }
                PrintStepMatrix(A, b);
            }

            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                {
                    sum += A[i, j] * x[j];
                }
                x[i] = (b[i] - sum) / A[i, i];
            }
            return x;
        }

        static void PrintStepMatrix(double[,] A, double[] b)
        {
            int n = b.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{A[i, j],8:F3} ");
                Console.WriteLine($" | {b[i],8:F3}");
            }
        }
    }

    // Метод Квадратного кореня
    public static class SquareRootSolver
    {
        public static double[] Solve(double[,] A, double[] b)
        {
            int n = b.Length;
            double[,] S = new double[n, n]; 
            double[] D = new double[n];    

            Console.WriteLine("Побудова матриці S (верхня трикутна)...");

            for (int i = 0; i < n; i++)
            {
                double sumDiag = 0;
                for (int k = 0; k < i; k++)
                    sumDiag += S[k, i] * S[k, i];

                double val = A[i, i] - sumDiag;
                if (val < 0) throw new Exception("Мінор від'ємний. Метод вимагає додатно визначеної матриці.");
                
                S[i, i] = Math.Sqrt(val);

                for (int j = i + 1; j < n; j++)
                {
                    double sumRow = 0;
                    for (int k = 0; k < i; k++)
                        sumRow += S[k, i] * S[k, j];

                    S[i, j] = (A[i, j] - sumRow) / S[i, i];
                }
                
                Console.WriteLine($"Рядок {i + 1} матриці S знайдено:");
                PrintTriangularMatrix(S, i + 1);
            }

            Console.WriteLine("\nРозв'язок S^T * y = b:");
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int k = 0; k < i; k++)
                    sum += S[k, i] * y[k]; 
                
                y[i] = (b[i] - sum) / S[i, i];
            }
            PrintVec(y, "y");

            Console.WriteLine("Розв'язок S * x = y:");
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int k = i + 1; k < n; k++)
                    sum += S[i, k] * x[k];
                
                x[i] = (y[i] - sum) / S[i, i];
            }
            PrintVec(x, "x");
            return x;
        }

        static void PrintTriangularMatrix(double[,] S, int rowsToShow)
        {
            int n = S.GetLength(0);
            for(int i=0; i<rowsToShow; i++)
            {
                for(int j=0; j<n; j++) Console.Write($"{S[i,j],8:F4} ");
                Console.WriteLine();
            }
        }
        static void PrintVec(double[] v, string name)
        {
            Console.Write($"{name} = [ ");
            foreach(var val in v) Console.Write($"{val:F4} ");
            Console.WriteLine("]");
        }
    }

    // Метод Якобі
    public static class JacobiSolver
    {
        public static (double[] Solution, int Iterations) Solve(double[,] A, double[] b, double epsilon)
        {
            int n = b.Length;
            double[] x = new double[n];
            double[] xNew = new double[n];
            int iterations = 0;
            double error;

            Console.WriteLine($"{"Ітер.",-6} {"x1",-10} {"x2",-10} {"x3",-10} {"x4",-10} {"Макс.похибка",-15}");

            do
            {
                iterations++;
                for (int i = 0; i < n; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j) sum += A[i, j] * x[j];
                    }
                    xNew[i] = (b[i] - sum) / A[i, i];
                }

                error = 0;
                for (int i = 0; i < n; i++)
                {
                    double diff = Math.Abs(xNew[i] - x[i]);
                    if (diff > error) error = diff;
                }

                Console.Write($"{iterations,-6} ");
                for(int i=0; i<n; i++) Console.Write($"{xNew[i],-10:F5} ");
                Console.WriteLine($"{error,-15:E4}");

                Array.Copy(xNew, x, n);

                if (iterations > 1000)
                {
                    Console.WriteLine("Перевищено ліміт ітерацій!");
                    break;
                }

            } while (error > epsilon);

            return (x, iterations);
        }
    }
}