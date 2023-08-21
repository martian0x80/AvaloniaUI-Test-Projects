using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NCalc;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Input;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private Dictionary<char, string> UnicodeOpsMap = new Dictionary<char, string>()
        {
            {(char)960, "Pi"},
            {(char)247, "/"},
            {(char)215, "*"},
            {(char)178, "**2"},
        };
        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Focus();
        }

        private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }


        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            MalformedInput.IsVisible = false;
            InputTextBox.Focus();
            var button = (sender as Button)!;

            if (button.Name == "ButtonXSquare")
            {
                InputTextBox.Text += (sender as Button)?.Tag?.ToString();
            }
            else
            {
                InputTextBox.Text += (sender as Button)?.Content?.ToString();
            }
//            InputTextBox.Text += (sender as Button)?.Content?.ToString();
            InputTextBox.CaretIndex = InputTextBox.Text.Length;
        }

        private void Button_OnClickBackSpace(object? sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text?.Length > 0)
            {
                InputTextBox.Text = InputTextBox.Text?.Remove(InputTextBox.Text.Length - 1);
            }
        }

        private void Button_OnClickMod(object? sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
            InputTextBox.Text += " " + (sender as Button)?.Tag?.ToString();
            InputTextBox.CaretIndex = InputTextBox.Text.Length - 1;
        }

        private string ReplaceUnicodeOps(string InputText)
        {
            Regex sqrtRegex = new Regex(@"\*?\u221a(\((\d+)\)|\d+)");
            MatchCollection sqrtMatches = sqrtRegex.Matches(InputText);
            foreach (Match i in sqrtMatches)
            {
                string? replaceString = "";
                GroupCollection groupCollection = i.Groups;
                int j = 1;
                while (groupCollection[^j].ToString() == "")
                {
                    j++;
                }

                if (InputText[0] != '\u221a')
                {
                    replaceString = $"*Sqrt({groupCollection[^j]})";
                }
                else
                {
                    replaceString = $"Sqrt({groupCollection[^j]})";
                }

                InputText = InputText.Replace(i.ToString(), replaceString);
                Console.WriteLine(i);
            }
            Console.WriteLine(InputText);
            foreach (char i in InputText)
            {
                if (UnicodeOpsMap.TryGetValue(i, out var value))
                {
                    InputText = InputText.Replace(i.ToString(), value);
                }
            }
            Console.WriteLine(InputText);
            return InputText;
        }

        private void CalculateButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text?.Length > 0)
            {
                string evalString = ReplaceUnicodeOps(InputTextBox.Text);
                Expression expression = new Expression(evalString);
                expression.EvaluateFunction += delegate(string name, FunctionArgs args)
                {
                    if (name == "mod")
                    {
                        // ReSharper disable once HeapView.BoxingAllocation
                        args.Result = (int)args.Parameters[0].Evaluate() % (int)args.Parameters[1].Evaluate();
                    }
                };
                try
                {
                    InputTextBox.Text = expression.Evaluate().ToString();
                } catch (NCalc.EvaluationException err)
                {
                    MalformedInput.IsVisible = true;
                }
            }
        }

        private void BackButton_OnHolding(object? sender, HoldingRoutedEventArgs e)
        {
            InputTextBox.Text = "";
        }
    }
    
}