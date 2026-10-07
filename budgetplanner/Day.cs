using System;
using System.Collections.Generic;

/// <summary>Stores a day's canonical name, planned budget, and spending.</summary>
public class Day
{
    private static readonly IReadOnlyDictionary<string, string[]> Names =
        new Dictionary<string, string[]>
        {
            ["Monday"] = new[] { "m", "M", "Mon", "Monday" },
            ["Tuesday"] = new[] { "t", "T", "Tue", "Tuesday" },
            ["Wednesday"] = new[] { "w", "W", "Wed", "Wednesday" },
            ["Thursday"] = new[] { "th", "TH", "Thu", "Thursday" },
            ["Friday"] = new[] { "f", "F", "Fri", "Friday" },
            ["Saturday"] = new[] { "st", "st", "Sat", "Saturday" },
            ["Sunday"] = new[] { "s", "S", "Sun", "Sunday" }
        };

    private string name = "";
    private Budget planned = new Budget(0);
    private Budget spent = new Budget(0);

    /// <summary>Resolves a case-insensitive day alias; unknown names leave the name empty.</summary>
    public Day(string name)
    {
        foreach (KeyValuePair<string, string[]> entry in Names)
        {
            foreach (string alias in entry.Value)
            {
                if (string.Equals(alias, name, StringComparison.OrdinalIgnoreCase))
                {
                    this.name = entry.Key;
                    return;
                }
            }
        }
    }

    /// <summary>Uses the supplied Budget object directly; changes to it are shared.</summary>
    public void SetBudget(Budget budget) => planned = budget;

    /// <summary>Replaces the planned budget with a new budget from the supplied amount.</summary>
    public void SetBudget(int budget) => planned = new Budget(budget);

    /// <summary>Replaces the planned budget with a new budget from the supplied amount.</summary>
    public void SetBudget(double budget) => planned = new Budget(budget);

    /// <summary>Adds to the planned budget.</summary>
    public void AddBudget(Budget amount) => planned.Add(amount);

    /// <summary>Adds to the planned budget.</summary>
    public void AddBudget(int amount) => planned.Add(amount);

    /// <summary>Adds to the planned budget. The operand is floored to two decimal places.</summary>
    public void AddBudget(double amount) => planned.Add(amount);

    /// <summary>Subtracts from the planned budget.</summary>
    public void SubBudget(Budget amount) => planned.Sub(amount);

    /// <summary>Subtracts from the planned budget.</summary>
    public void SubBudget(int amount) => planned.Sub(amount);

    /// <summary>Subtracts from the planned budget. The operand is floored to two decimal places.</summary>
    public void SubBudget(double amount) => planned.Sub(amount);

    /// <summary>Adds to recorded spending.</summary>
    public void AddSpent(Budget amount) => spent.Add(amount);

    /// <summary>Adds to recorded spending.</summary>
    public void AddSpent(int amount) => spent.Add(amount);

    /// <summary>Adds to recorded spending. The operand is floored to two decimal places.</summary>
    public void AddSpent(double amount) => spent.Add(amount);

    /// <summary>Returns the actual mutable planned budget.</summary>
    public Budget GetPlannedBudget() => planned;

    /// <summary>Returns the actual mutable spending budget.</summary>
    public Budget GetSpent() => spent;

    /// <summary>Returns the canonical day name or the empty string for an unknown alias.</summary>
    public string GetName() => name;
}
