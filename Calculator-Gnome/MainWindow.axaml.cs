using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NCalc;

namespace Calculator;

public partial class MainWindow : Window
{
    private readonly Dictionary<char, string> _unicodeOpsMap = new()
    {
        { (char)960, "Pi" },
        { (char)247, "/" },
        { (char)215, "*" },
        { (char)178, "**2" }
    };

    private FixedQueue<KeyValuePair<string?, string?>> _historyResultBox = new FixedQueue<KeyValuePair<string?, string?>>(3);

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
        var button = sender as Button;

        if (button?.Name == "ButtonXSquare")
        {
//                InputTextBox.Text += (sender as Button)?.Tag?.ToString();
            if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
                InputTextBox.Text = InputTextBox.Text?.Insert(InputTextBox.CaretIndex,
                    (sender as Button)?.Tag?.ToString() ?? string.Empty);
            else
                InputTextBox.Text += (sender as Button)?.Tag?.ToString();
        }
        else
        {
//                InputTextBox.Text += (sender as Button)?.Content?.ToString();
            if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
                InputTextBox.Text = InputTextBox.Text?.Insert(InputTextBox.CaretIndex,
                    (sender as Button)?.Content?.ToString() ?? string.Empty);
            else
                InputTextBox.Text += (sender as Button)?.Content?.ToString();
        }

//            InputTextBox.Text += (sender as Button)?.Content?.ToString();
        if (InputTextBox.CaretIndex < InputTextBox.Text?.Length)
            ChangeCaretIndexPosRelativeToItself(1);
        else
            ChangeCaretIndexPosToEndOfInputText();
    }

    private void Button_OnClickBackSpace(object? sender, RoutedEventArgs e)
    {
        if (InputTextBox.Text?.Length > 0)
        {
            if (InputTextBox.CaretIndex > 0)
                InputTextBox.Text = InputTextBox.Text?.Remove(InputTextBox.CaretIndex - 1, 1);

            if (InputTextBox.Text?.Length > InputTextBox.CaretIndex) InputTextBox.CaretIndex -= 1;
        }
    }
    
    private void Button_OnClickMod(object? sender, RoutedEventArgs e)
    {
        InputTextBox.Focus();
//          Adds mod() at the CaretIndex
        InputTextBox.Text =
            InputTextBox.Text?.Insert(InputTextBox.CaretIndex, (sender as Button)?.Tag?.ToString()!);
//          Adds mod() at the end of the InputTextBox.Text
//          InputTextBox.Text += " " + (sender as Button)?.Tag?.ToString();
        ChangeCaretIndexPosRelativeToItself(5);
    }

    private string ReplaceUnicodeOps(string inputText)
    {
        Console.Write($"{inputText} => ");

//          Sqrt

//          Simple regex matches but can't handle nested expressions
//          Regex sqrtRegex = new Regex(@"\*?\u221a(\((\d+)\)|\d+)");

//          ECMAScript (Javascript) regex v1, since .net regex engine puked with non-balancing groups
        var sqrtRegex = new Regex(@"\u00d7?\u221a(\((?:[^)(]|\((?:[^)(]|\((?:[^)(]|\([^)(]*\))*\))*\))*\)|\d+)");
        var sqrtMatches = sqrtRegex.Matches(inputText);
        foreach (Match i in sqrtMatches)
        {
            var replaceString = "";
            var groupCollection = i.Groups;
            if (groupCollection[1].Success)
            {
                if (inputText[0] != '\u221a')
                    replaceString = $"*Sqrt({groupCollection[1]})";
                else
                    replaceString = $"Sqrt({groupCollection[1]})";
                // Deprecated, was horrible i know
                /*GroupCollection groupCollection = i.Groups;
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
                }*/

                inputText = inputText.Replace(i.ToString(), replaceString);
            }
        }

//          Mod

        var modRegex =
            new Regex(
                @"\u00d7?(\((?:[^)(]|\((?:[^)(]|\((?:[^)(]|\([^)(]*\))*\))*\))*\)|\d+)\s*mod\s*(\((?:[^)(]|\((?:[^)(]|\((?:[^)(]|\([^)(]*\))*\))*\))*\)|\d+)");
        var modMatches = modRegex.Matches(inputText);
        foreach (Match i in modMatches)
        {
            var replaceString = "";
            var groupCollection = i.Groups;
            if (groupCollection[2].Success && groupCollection[1].Success)
            {
                replaceString = $"*mod({groupCollection[1]},{groupCollection[2]})";
                inputText = inputText.Replace(i.ToString(), replaceString);
                if (inputText[0] == '*') inputText = inputText.Substring(1);
            }
        }

        // Unicode to common operators
        Console.Write($"{inputText} => ");
        var j = 'a';
        foreach (var i in inputText)
        {
            if (_unicodeOpsMap.TryGetValue(i, out var value))
            {
                if (inputText[0] != (char)960 && i == (char)960 && !new[] {'(', '\u221a'}.Contains(j))
                    inputText = inputText.Replace(i.ToString(), "*" + $"({value})");
                else
                    inputText = inputText.Replace(i.ToString(), $"{value}");
            }
            
        }

        inputText = inputText.Replace("(*", "(");
        inputText = inputText.Replace("\u221a*", "\u221a");
        
        Console.WriteLine(inputText);
        return inputText;
    }

    private void CalculateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (InputTextBox.Text?.Length > 0)
        {   
            // The first run adds parenthesis to Pi or \d+ (and converts that to Sqrt in case of \d+)
            var evalString = ReplaceUnicodeOps(InputTextBox.Text);
            /*
            /* The second run checks for non-ascii characters and the case when the parameters are parsed but not the full
            /* regex match, this is a consequence of my inability to make the sqrtRegex match \u221aPi directly, but it takes
            /* the second run to read the parenthesis introduced in first step
            */
            evalString = evalString.Length != System.Text.Encoding.UTF8.GetByteCount(evalString)
                ? ReplaceUnicodeOps(evalString)
                : evalString;
            var expression = new Expression(evalString);

            expression.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                if (name == "mod")
                    // ReSharper disable once HeapView.BoxingAllocation
                    args.Result = (int)args.Parameters[0].Evaluate() % (int)args.Parameters[1].Evaluate();
            };
            expression.EvaluateParameter += delegate(string pi, ParameterArgs args)
            {
                if (pi == "Pi") args.Result = double.Pi;
            };
            bool successFlag = true;
            string InputTextBoxTextPreserved = InputTextBox.Text;
            string? result = "";
            try
            {
                result = expression.Evaluate().ToString();
                InputTextBox.Text = result;
                ChangeCaretIndexPosToEndOfInputText();
                successFlag = true;
            }
            catch (Exception err)
            {
                MalformedInput.IsVisible = true;
                Console.WriteLine($"Caught a fish: {err.Message}\nSource:: {err.Source}\n");
                successFlag = false;
            }
            finally
            {
                if (successFlag)
                {
                    _historyResultBox.Enqueue(KeyValuePair.Create<string?, string?>(InputTextBoxTextPreserved, result!));

                    HistoryBox3.Text = _historyResultBox.Peek().Key;
                    ResultBox3.Text = _historyResultBox.Peek().Value;
                    
                    var queueArr = _historyResultBox.ToArray();

                    HistoryBox2.Text = queueArr.ElementAtOrDefault(1).Key ?? String.Empty;
                    ResultBox2.Text = queueArr.ElementAtOrDefault(1).Value ?? String.Empty;

                    HistoryBox1.Text = queueArr.ElementAtOrDefault(2).Key ?? String.Empty;
                    ResultBox1.Text = queueArr.ElementAtOrDefault(2).Value ?? String.Empty;
                    
                }
            }
        }
        
        
    }


    private void BackButton_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        InputTextBox.Text = "";
    }
}

class FixedQueue<T> : Queue<T>
{
    public int Limit { get; set; }

    public FixedQueue(int limit) : base(limit)
    {
        Limit = limit;
    }

    public new void Enqueue(T item)
    {
        while (Count >= Limit)
        {
            base.Dequeue();
        }

        base.Enqueue(item);
    }
    
}