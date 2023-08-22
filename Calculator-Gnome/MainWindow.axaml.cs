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
        private readonly Dictionary<char, string> _unicodeOpsMap = new Dictionary<char, string>()
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

        private void ChangeCaretIndexPosToEndOfInputText(int offset = 0)
        {
            InputTextBox.CaretIndex = InputTextBox.Text?.Length > offset ? InputTextBox.Text.Length - offset : 0;
        }

        private void ChangeCaretIndexPosRelativeToItself(int fOffset = 0)
        {
            InputTextBox.CaretIndex += fOffset;

        }

        private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }

// This took me a lifetime to figure out
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            MalformedInput.IsVisible = false;
            InputTextBox.Focus();
            var button = (sender as Button);

            if (button?.Name == "ButtonXSquare")
            {
//                InputTextBox.Text += (sender as Button)?.Tag?.ToString();
                if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
                {
                    InputTextBox.Text = InputTextBox.Text?.Insert(InputTextBox.CaretIndex, (sender as Button)?.Tag?.ToString() ?? string.Empty);
                }
                else
                {
                    InputTextBox.Text += (sender as Button)?.Tag?.ToString();
                }
            }
            else
            {
//                InputTextBox.Text += (sender as Button)?.Content?.ToString();
                if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
                {
                    InputTextBox.Text = InputTextBox.Text?.Insert(InputTextBox.CaretIndex, (sender as Button)?.Content?.ToString() ?? string.Empty);
                }
                else
                {
                    InputTextBox.Text += (sender as Button)?.Content?.ToString();

                }
            }
//            InputTextBox.Text += (sender as Button)?.Content?.ToString();
            if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
            {
                ChangeCaretIndexPosRelativeToItself(1);
            }
            else
            {
                ChangeCaretIndexPosToEndOfInputText(0);
            }
        }

        private void Button_OnClickBackSpace(object? sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text?.Length > 0)
            {
                if (InputTextBox.CaretIndex > 0)
                {
                    InputTextBox.Text = InputTextBox.Text?.Remove(InputTextBox.CaretIndex - 1, 1);
                }
                if (InputTextBox.Text?.Length > InputTextBox.CaretIndex)
                {
                    InputTextBox.CaretIndex -= 1;
                }
            }
        }

        //ToDo: Regex Mod or multivariable mod
        private void Button_OnClickMod(object? sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
//          Adds mod() at the CaretIndex
            InputTextBox.Text = InputTextBox.Text?.Insert(InputTextBox.CaretIndex, $"{(char)215}" + (sender as Button)?.Tag?.ToString());
//          Adds mod() at the end of the InputTextBox.Text
//          InputTextBox.Text += " " + (sender as Button)?.Tag?.ToString();
            ChangeCaretIndexPosRelativeToItself(5);
        }

        private string ReplaceUnicodeOps(string inputText)
        {
            Regex sqrtRegex = new Regex(@"\*?\u221a(\((\d+)\)|\d+)");
            MatchCollection sqrtMatches = sqrtRegex.Matches(inputText);
            foreach (Match i in sqrtMatches)
            {
                string? replaceString = "";
                GroupCollection groupCollection = i.Groups;
                int j = 1;
                while (groupCollection[^j].ToString() == "")
                {
                    j++;
                }

                if (inputText[0] != '\u221a')
                {
                    replaceString = $"*Sqrt({groupCollection[^j]})";
                }
                else
                {
                    replaceString = $"Sqrt({groupCollection[^j]})";
                }

                inputText = inputText.Replace(i.ToString(), replaceString);
                Console.Write(i);
            }
            // Pi
            //
            Console.Write($"{inputText} => ");
            foreach (char i in inputText)
            {
                if (_unicodeOpsMap.TryGetValue(i, out var value))
                {
                    inputText = inputText.Replace(i.ToString(), value);
                }
            }
            Console.WriteLine(inputText);
            return inputText;
        }

        private void CalculateButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text?.Length > 0)
            {
                string evalString = ReplaceUnicodeOps(InputTextBox.Text);
                Expression expression = new Expression(evalString);
                // ToDo: Regex mod or multivariable mod
                expression.EvaluateFunction += delegate(string name, FunctionArgs args)
                {
                    if (name == "mod")
                    {
                        // ReSharper disable once HeapView.BoxingAllocation
                        args.Result = (int)args.Parameters[0].Evaluate() % (int)args.Parameters[1].Evaluate();
                    }
                };
                expression.EvaluateParameter += delegate(string pi, ParameterArgs args)
                {
                    if (pi == "Pi")
                    {
                        args.Result = Double.Pi;
                    }
                };
                try
                {
                    InputTextBox.Text = expression.Evaluate().ToString();
                    ChangeCaretIndexPosToEndOfInputText(0);
                } catch (NCalc.EvaluationException err)
                {
                    MalformedInput.IsVisible = true;
                    Console.WriteLine($"Caught a fish: {err}\n");
                }
            }
        }

        private void BackButton_OnHolding(object? sender, HoldingRoutedEventArgs e)
        {
            InputTextBox.Text = "";
        }
    }
    
}
