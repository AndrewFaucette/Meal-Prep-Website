using System;
using System.Collections.Generic;

/// <summary>Holds the ordered, mutable collection of weekly plans.</summary>
public class Plan
{
    private List<Week> weeks = new List<Week>();

    /// <summary>Creates an empty plan.</summary>
    public Plan() { }

    /// <summary>Creates a one-week plan and performs the original duplicate-name check.</summary>
    /// <remarks>The check includes the newly added week itself, so it always renames that week to Week 1.</remarks>
    public Plan(Week theWeek)
    {
        weeks.Add(theWeek);
        foreach (Week week in weeks)
        {
            if (string.Equals(week.GetName(), theWeek.GetName(), StringComparison.Ordinal))
            {
                theWeek.SetName("Week " + weeks.Count);
                return;
            }
        }
    }

    /// <summary>Uses the supplied list directly; subsequent list changes are shared with the caller.</summary>
    public Plan(List<Week> weeks) => this.weeks = weeks;

    /// <summary>Returns the week at a zero-based index; an invalid index throws.</summary>
    public Week GetWeek(int i) => weeks[i];

    /// <summary>Returns the actual mutable week list, matching the Java API.</summary>
    public List<Week> GetWeeks() => weeks;

    /// <summary>Appends a week without validating or renaming it.</summary>
    public void AddWeek(Week week) => weeks.Add(week);

    /// <summary>Serializes weeks with LF separators and no final newline.</summary>
    /// <remarks>The original reference-based separator check is retained for duplicate Week objects.</remarks>
    public string FileFormat()
    {
        string format = "";
        foreach (Week week in weeks)
        {
            format += week.FileFormat();
            if (!ReferenceEquals(weeks[weeks.Count - 1], week))
                format += "\n";
        }
        return format;
    }

    /// <summary>Prints each week's one-based selection number, name, spending, and planned total.</summary>
    public void DisplayAllWeeks()
    {
        string format = "";
        foreach (Week week in weeks)
            format += "    " + (weeks.IndexOf(week) + 1) + "." + week.GetName() + ": "
                + week.GetSepntTotal().StringFormat() + "/" + week.GetTotalBudget().StringFormat() + "\n";
        Console.WriteLine(format);
    }
}
