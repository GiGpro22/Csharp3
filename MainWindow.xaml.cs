using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp4dz1
{
    public partial class MainWindow : Window
    {
        bool isResultShown = false;
        CultureInfo format = new CultureInfo("ru-RU");

        public MainWindow()
        {
            InitializeComponent();
            Clear();
        }

        void Clear()
        {
            MainDisplay.Text = "0";
            SecondaryDisplay.Text = "";
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var num = ((Button)sender).Content.ToString();
            if (isResultShown)
            {
                SecondaryDisplay.Text = num;
                isResultShown = false;
            }
            else
            {
                if (num == ",")
                {
                    var last = GetLastNumber(SecondaryDisplay.Text);
                    if (last.Contains(",")) return;
                    if (string.IsNullOrEmpty(last) || last == "-")
                        SecondaryDisplay.Text += "0";
                }
                SecondaryDisplay.Text += num;
            }
            MainDisplay.Text = "0";
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            var op = ((Button)sender).Tag.ToString();
            if (isResultShown)
            {
                SecondaryDisplay.Text = MainDisplay.Text;
                isResultShown = false;
            }
            if (SecondaryDisplay.Text.Length == 0 && op == "-")
            {
                SecondaryDisplay.Text = "-";
                MainDisplay.Text = "0";
                return;
            }
            if (SecondaryDisplay.Text.Length > 0)
            {
                var last = SecondaryDisplay.Text.TrimEnd();
                if (last.Length > 0 && !IsOperator(last[last.Length - 1].ToString()) && last != "(")
                {
                    SecondaryDisplay.Text += " " + op + " ";
                }
            }
            MainDisplay.Text = "0";
        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            var func = ((Button)sender).Tag.ToString();
            if (isResultShown)
            {
                SecondaryDisplay.Text = MainDisplay.Text;
                isResultShown = false;
            }
            switch (func)
            {
                case "Sin": SecondaryDisplay.Text += "sin("; break;
                case "Cos": SecondaryDisplay.Text += "cos("; break;
                case "Tan": SecondaryDisplay.Text += "tan("; break;
                case "Square": SecondaryDisplay.Text += "sqr("; break;
                case "Sqrt": SecondaryDisplay.Text += "sqrt("; break;
                case "Inverse": SecondaryDisplay.Text += "inv("; break;
                case "Abs": SecondaryDisplay.Text += "abs("; break;
                case "Factorial": SecondaryDisplay.Text += "fact("; break;
                case "Log": SecondaryDisplay.Text += "log("; break;
                case "Ln": SecondaryDisplay.Text += "ln("; break;
                case "TenPower": SecondaryDisplay.Text += "10^("; break;
            }
            MainDisplay.Text = "0";
        }

        private void ConstantButton_Click(object sender, RoutedEventArgs e)
        {
            var c = ((Button)sender).Tag.ToString();
            if (isResultShown)
            {
                SecondaryDisplay.Text = "";
                isResultShown = false;
            }
            switch (c)
            {
                case "PI": SecondaryDisplay.Text += Math.PI.ToString(format); break;
                case "E": SecondaryDisplay.Text += Math.E.ToString(format); break;
            }
            MainDisplay.Text = "0";
        }

        private void ParenthesisButton_Click(object sender, RoutedEventArgs e)
        {
            string parenthesis = ((Button)sender).Tag.ToString();
            if (isResultShown)
            {
                SecondaryDisplay.Text = "";
                isResultShown = false;
            }
            SecondaryDisplay.Text += parenthesis;
            MainDisplay.Text = "0";
        }

        private void SignChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryDisplay.Text.Length > 0)
            {
                var last = GetLastNumber(SecondaryDisplay.Text);
                if (!string.IsNullOrEmpty(last))
                {
                    int idx = SecondaryDisplay.Text.LastIndexOf(last);
                    if (idx >= 0)
                    {
                        var newNum = last.StartsWith("-") ? last.Substring(1) : "-" + last;
                        SecondaryDisplay.Text = SecondaryDisplay.Text.Substring(0, idx) + newNum +
                            SecondaryDisplay.Text.Substring(idx + last.Length);
                    }
                }
                else if (SecondaryDisplay.Text.EndsWith(" "))
                    SecondaryDisplay.Text += "-";
                else if (SecondaryDisplay.Text.Length == 0)
                    SecondaryDisplay.Text = "-";
            }
            MainDisplay.Text = "0";
        }

        private void ClearEntryButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            isResultShown = false;
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryDisplay.Text.Length > 0)
            {
                SecondaryDisplay.Text = SecondaryDisplay.Text.Substring(0, SecondaryDisplay.Text.Length - 1);
                if (SecondaryDisplay.Text.Length == 0)
                    MainDisplay.Text = "0";
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SecondaryDisplay.Text))
                {
                    MainDisplay.Text = "0";
                    return;
                }

                var result = EvaluateExpression(SecondaryDisplay.Text);
                string text;

                if (Math.Abs(result - Math.Round(result)) < 0.0000001)
                    text = result.ToString("0", format);
                else
                    text = result.ToString("G10", format).TrimEnd('0').TrimEnd(',');

                MainDisplay.Text = text;
                isResultShown = true;
            }
            catch (Exception ex)
            {
                MainDisplay.Text = "Ошибка: " + ex.Message;
                isResultShown = true;
            }
        }

        string GetLastNumber(string expr)
        {
            var matches = Regex.Matches(expr, @"-?\d+[,\.]?\d*");
            return matches.Count > 0 ? matches[matches.Count - 1].Value : "";
        }

        double EvaluateExpression(string expr)
        {
            try
            {
                return CalculateRPN(ConvertToRPN(expr));
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка: " + ex.Message);
            }
        }

        List<string> ConvertToRPN(string expr)
        {
            var output = new List<string>();
            var ops = new Stack<string>();

            var pattern = @"(-?\d+[,\.]?\d*)|([+\-*/^()])|(\bsin\b|\bcos\b|\btan\b|\bsqr\b|\bsqrt\b|\binv\b|\babs\b|\bfact\b|\blog\b|\bln\b|\b10\^\b)";
            var matches = Regex.Matches(expr.Trim(), pattern);

            foreach (Match m in matches)
            {
                var token = m.Value.Trim();
                if (string.IsNullOrEmpty(token)) continue;

                if (double.TryParse(token.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                {
                    output.Add(num.ToString(CultureInfo.InvariantCulture));
                    continue;
                }

                if (IsFunction(token))
                    ops.Push(token);
                else if (token == "(")
                    ops.Push(token);
                else if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                        output.Add(ops.Pop());
                    if (ops.Count > 0) ops.Pop();
                }
                else if (IsOperator(token))
                {
                    while (ops.Count > 0 && ops.Peek() != "(" && GetPrecedence(ops.Peek()) >= GetPrecedence(token))
                        output.Add(ops.Pop());
                    ops.Push(token);
                }
            }

            while (ops.Count > 0)
            {
                var op = ops.Pop();
                if (op == "(") throw new Exception("Непарные скобки");
                output.Add(op);
            }

            return output;
        }

        double CalculateRPN(List<string> rpn)
        {
            if (rpn.Count == 0) throw new Exception("Пустое выражение");

            var nums = new Stack<double>();

            foreach (var token in rpn)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                {
                    nums.Push(num);
                    continue;
                }

                if (IsFunction(token))
                {
                    if (nums.Count < 1) throw new Exception($"Мало аргументов для {token}");
                    nums.Push(ApplyFunction(token, nums.Pop()));
                }
                else if (IsOperator(token))
                {
                    if (nums.Count < 2) throw new Exception($"Мало аргументов для {token}");
                    var b = nums.Pop();
                    var a = nums.Pop();
                    nums.Push(ApplyOperator(token, a, b));
                }
            }

            if (nums.Count != 1) throw new Exception("Неверное выражение");
            return nums.Pop();
        }

        bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" ||
                   token == "sqr" || token == "sqrt" || token == "inv" ||
                   token == "abs" || token == "fact" || token == "log" ||
                   token == "ln" || token == "10^";
        }

        bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" ||
                   token == "/" || token == "^";
        }

        int GetPrecedence(string op)
        {
            switch (op)
            {
                case "+":
                case "-": return 1;
                case "*":
                case "/": return 2;
                case "^": return 3;
                default: return 0;
            }
        }

        double ApplyFunction(string func, double x)
        {
            try
            {
                switch (func)
                {
                    case "sin": return Math.Sin(x);
                    case "cos": return Math.Cos(x);
                    case "tan": return Math.Tan(x);
                    case "sqr": return x * x;
                    case "sqrt":
                        if (x < 0) throw new Exception("Корень из отрицательного");
                        return Math.Sqrt(x);
                    case "inv":
                        if (x == 0) throw new Exception("Деление на ноль");
                        return 1.0 / x;
                    case "abs": return Math.Abs(x);
                    case "fact":
                        if (x < 0) throw new Exception("Факториал отрицательного");
                        if (x > 170) throw new Exception("Слишком большое число");
                        return Factorial((int)x);
                    case "log":
                        if (x <= 0) throw new Exception("Логарифм неположительного");
                        return Math.Log10(x);
                    case "ln":
                        if (x <= 0) throw new Exception("Логарифм неположительного");
                        return Math.Log(x);
                    case "10^": return Math.Pow(10, x);
                    default: throw new Exception("Неизвестная функция");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в {func}: {ex.Message}");
            }
        }

        double ApplyOperator(string op, double a, double b)
        {
            try
            {
                switch (op)
                {
                    case "+": return a + b;
                    case "-": return a - b;
                    case "*": return a * b;
                    case "/":
                        if (b == 0) throw new Exception("Деление на ноль");
                        return a / b;
                    case "^":
                        if (a == 0 && b < 0) throw new Exception("Ноль в минус степени");
                        return Math.Pow(a, b);
                    default: throw new Exception("Неизвестный оператор");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в {op}: {ex.Message}");
            }
        }

        int Factorial(int n)
        {
            if (n < 0) throw new Exception("Факториал отрицательного");
            if (n == 0 || n == 1) return 1;
            return n * Factorial(n - 1);
        }
    }
}