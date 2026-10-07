using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

/// <summary>Holds seven days, cached totals, and interactive weekly budgeting operations.</summary>
public class Week
{
    private readonly List<Day> days = new List<Day>(7);
    private readonly Budget planned = new Budget(0);
    private readonly Budget spent = new Budget(0);
    private string name = "Week 01";
    private bool planComplete;

    /// <summary>Creates Monday through Sunday and initializes the planned total.</summary>
    public Week()
    {
        SetupDays();
        SetTotalBudget();
    }

    /// <summary>Creates a named week, initializes its days, and invokes the original total calculations.</summary>
    public Week(string name)
    {
        this.name = name;
        SetupDays();
        SetTotalBudget();
        SetSpentTotal();
    }

    /// <summary>Appends the seven days in Monday-to-Sunday order.</summary>
    private void SetupDays()
    {
        foreach (string dayName in new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" })
            days.Add(new Day(dayName));
    }

    /// <summary>Returns the day at a zero-based index (Monday is 0); invalid indices throw.</summary>
    public Day GetDay(int i) => days[i];

    /// <summary>Replaces the planned budget for the zero-based day index. The Budget reference is shared.</summary>
    public void SetDailyBudget(int day, Budget amount) => days[day].SetBudget(amount);

    /// <summary>Replaces the planned budget for the zero-based day index.</summary>
    public void SetDailyBudget(int day, int amount) => days[day].SetBudget(amount);

    /// <summary>Replaces the planned budget for the zero-based day index.</summary>
    public void SetDailyBudget(int day, double amount) => days[day].SetBudget(amount);

    /// <summary>Adds to the planned budget for the zero-based day index.</summary>
    public void AddDailyBudget(int day, Budget amount) => days[day].AddBudget(amount);

    /// <summary>Adds to the planned budget for the zero-based day index.</summary>
    public void AddDailyBudget(int day, int amount) => days[day].AddBudget(amount);

    /// <summary>Adds to the planned budget for the zero-based day index.</summary>
    public void AddDailyBudget(int day, double amount) => days[day].AddBudget(amount);

    /// <summary>Subtracts from the planned budget for the zero-based day index.</summary>
    public void SubDailyBudget(int day, Budget amount) => days[day].SubBudget(amount);

    /// <summary>Subtracts from the planned budget for the zero-based day index.</summary>
    public void SubDailyBudget(int day, int amount) => days[day].SubBudget(amount);

    /// <summary>Subtracts from the planned budget for the zero-based day index.</summary>
    public void SubDailyBudget(int day, double amount) => days[day].SubBudget(amount);

    /// <summary>Adds to recorded spending for the zero-based day index.</summary>
    public void AddDailySpent(int day, Budget amount) => days[day].AddSpent(amount);

    /// <summary>Adds to recorded spending for the zero-based day index.</summary>
    public void AddDailySpent(int day, int amount) => days[day].AddSpent(amount);

    /// <summary>Adds to recorded spending for the zero-based day index.</summary>
    public void AddDailySpent(int day, double amount) => days[day].AddSpent(amount);

    /// <summary>Returns the mutable spending budget for the zero-based day index.</summary>
    public Budget GetDailySpent(int day) => days[day].GetSpent();

    /// <summary>Returns the mutable planned budget for the zero-based day index.</summary>
    public Budget GetDailyBudget(int day) => days[day].GetPlannedBudget();

    /// <summary>Adds to daily spending, despite the original method name suggesting replacement.</summary>
    public void SetSpentDaily(int day, double amount) => days[day].AddSpent(amount);

    /// <summary>Parses a spent/planned record, adds its spending, and replaces the day's planned budget.</summary>
    /// <remarks>Missing fields or invalid invariant-culture numbers throw; extra fields are ignored.</remarks>
    public void SetSpentBudgetDaily(int day, string record)
    {
        string[] split = record.Split('/');
        double spentAmount = double.Parse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture);
        double plannedAmount = double.Parse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture);
        SetSpentDaily(day, spentAmount);
        SetDailyBudget(day, plannedAmount);
    }

    /// <summary>Retains the original calculation that adds spending into the planned total.</summary>
    /// <remarks>Source bug: this neither resets a total nor updates the spent field.</remarks>
    public void SetSpentTotal()
    {
        foreach (Day day in days)
            planned.Add(day.GetSpent());
    }

    /// <summary>Invokes the original spending calculation and returns the unchanged spending total.</summary>
    /// <remarks>The misspelling is retained for compatibility; the result stays zero in this application.</remarks>
    public Budget GetSepntTotal()
    {
        SetSpentTotal();
        return spent;
    }

    /// <summary>Resets and recalculates the planned total from all seven daily budgets.</summary>
    public void SetTotalBudget()
    {
        planned.SetAmount(0);
        foreach (Day day in days)
            planned.Add(day.GetPlannedBudget());
    }

    /// <summary>Recalculates and returns the actual mutable planned total.</summary>
    public Budget GetTotalBudget()
    {
        SetTotalBudget();
        return planned;
    }

    /// <summary>Replaces the week name without validating it.</summary>
    public void SetName(string name) => this.name = name;

    /// <summary>Returns the week name.</summary>
    public string GetName() => name;

    /// <summary>Returns completion text using the original formatted-zero test.</summary>
    /// <remarks>Value comparison replaces Java reference comparison, but zero formats as 0.0, not 0.00.</remarks>
    public string Status()
    {
        if (!planComplete)
        {
            foreach (Day day in days)
                if (string.Equals(day.GetPlannedBudget().StringFormat(), "0.00", StringComparison.Ordinal))
                    return "Not completed";
        }
        return "Completed";
    }

    /// <summary>Prompts for seven integer budgets and adds this week to the plan when confirmed.</summary>
    /// <remarks>Original behavior: rejected or partially failed attempts retain additions to daily budgets.</remarks>
    public void SetWeeklyBudget(Plan plan)
    {
        bool notDecided = true;
        while (notDecided)
        {
            try
            {
                Console.WriteLine("How much money are you planning to spend? Enter a integer number.");
                foreach (Day day in days)
                {
                    Console.WriteLine(day.GetName() + ": ");
                    day.AddBudget(Options.ReadInteger());
                }
                Console.WriteLine("You will spend $" + GetTotalBudget().StringFormat() + " in this week.");
                notDecided = !ConfirmBudget(plan);
            }
            catch (Exception e) when (e is not EndOfStreamException)
            {
                Console.WriteLine("Error. Let's plan the budget again.");
            }
        }
    }

    /// <summary>Prompts for one integer budget, sets it on every day, and saves the week when confirmed.</summary>
    public void SetWeeklyBudgetSame(Plan plan)
    {
        bool notDecided = true;
        while (notDecided)
        {
            try
            {
                Console.WriteLine("How much money are you planning to spend every day this week? Enter a integer number.");
                int amount = Options.ReadInteger();
                foreach (Day day in days)
                    day.SetBudget(amount);
                DisplayDays();
                Console.WriteLine("You will spend $" + GetTotalBudget().StringFormat() + " in this week.");
                notDecided = !ConfirmBudget(plan);
            }
            catch (Exception e) when (e is not EndOfStreamException)
            {
                Console.WriteLine("Error. Let's plan the budget again.");
            }
        }
    }

    /// <summary>Reads y/n confirmation, marks and appends this week on y, or requests replanning on n.</summary>
    private bool ConfirmBudget(Plan plan)
    {
        Console.WriteLine("Are you good with this budget?(y/n)");
        while (true)
        {
            string response = Options.ReadInput().Trim();
            if (string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Perfect! Planning is done.");
                plan.AddWeek(this);
                planComplete = true;
                return true;
            }
            if (string.Equals(response, "n", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Ok! Let's plan the budget again.");
                return false;
            }
            Console.WriteLine("Invalid input. Are you good with this budget?(y/n)");
        }
    }

    /// <summary>Prints numbered days with spending/planned amounts.</summary>
    public void DisplayDays()
    {
        string format = "";
        foreach (Day day in days)
            format += "    " + (days.IndexOf(day) + 1) + ". " + day.GetName() + ": "
                + day.GetSpent().StringFormat() + "/" + day.GetPlannedBudget().StringFormat() + "\n";
        Console.WriteLine(format);
    }

    /// <summary>Serializes name, spending total, planned total, and seven spent/planned fields.</summary>
    /// <remarks>Names are not CSV-escaped, and the original spending-total bug is retained.</remarks>
    public string FileFormat()
    {
        string format = name + "," + GetSepntTotal().StringFormat() + "," + GetTotalBudget().StringFormat();
        for (int i = 0; i < 7; i++)
            format += "," + GetDailySpent(i).StringFormat() + "/" + GetDailyBudget(i).StringFormat();
        return format;
    }
}
