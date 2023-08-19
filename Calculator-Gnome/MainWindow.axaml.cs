using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /*private decimal? calc(decimal? num1, decimal? num2)
    {
        return num1 + num2;
    }
    public void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;
        Answer.Text = calc(num1.Value, num2.Value).ToString();
    }

    private void Num_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        var spinbox = (NumericUpDown)sender!;
        Answer.Text = calc(num1.Value, num2.Value).ToString();
    }*/
    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        InputTextBox.Focus();
    }
    

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
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
    }

    private void InputTextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
//        throw new System.NotImplementedException();
    }

    private void Button_OnClickBackSpace(object? sender, RoutedEventArgs e)
    {
//        throw new System.NotImplementedException();
    }
}