using System;
using System.Globalization;

/// <summary>Stores a mutable money amount using the original program's double arithmetic.</summary>
public class Budget
{
    private double amount;

    /// <summary>Creates a budget from an integer amount.</summary>
    public Budget(int amount) => this.amount = amount;

    /// <summary>Creates a budget, flooring the supplied amount to two decimal places.</summary>
    public Budget(double amount) => this.amount = Math.Floor(amount * 100) / 100;

    /// <summary>Replaces the amount with an integer value.</summary>
    public void SetAmount(int amount) => this.amount = amount;

    /// <summary>Replaces the amount after flooring it to two decimal places.</summary>
    public void SetAmount(double amount) => this.amount = Math.Floor(amount * 100) / 100;

    /// <summary>Returns the stored amount without further rounding.</summary>
    public double GetAmount() => amount;

    /// <summary>Adds the other budget's stored amount without modifying that budget.</summary>
    public void Add(Budget a) => amount += a.GetAmount();

    /// <summary>Adds an integer amount.</summary>
    public void Add(int a) => amount += a;

    /// <summary>Adds a double amount after flooring the operand to two decimal places.</summary>
    public void Add(double a) => amount += Math.Floor(a * 100) / 100;

    /// <summary>Subtracts the other budget's stored amount without modifying that budget.</summary>
    public void Sub(Budget a) => amount -= a.GetAmount();

    /// <summary>Subtracts an integer amount.</summary>
    public void Sub(int a) => amount -= a;

    /// <summary>Subtracts a double amount after flooring the operand to two decimal places.</summary>
    public void Sub(double a) => amount -= Math.Floor(a * 100) / 100;

    /// <summary>Returns culture-independent text, retaining Java's decimal suffix for whole values.</summary>
    /// <remarks>This is not fixed two-decimal currency formatting. Scientific notation can differ from Java.</remarks>
    public string StringFormat()
    {
        string text = amount.ToString("R", CultureInfo.InvariantCulture);
        return double.IsFinite(amount) && !text.Contains('.') && !text.Contains('E')
            ? text + ".0"
            : text;
    }
}
