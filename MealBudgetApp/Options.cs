using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>Runs the menu and manages creation, editing, saving, and loading of the current plan.</summary>
public class Options
{
    private const string Menu = "    1. Create a week\n    2. Resume a week\n    3. Save the current plan in a file\n    4. Load a file\n    5. Exit\nEnter a number from the options.";
    private Plan currentPlan = new Plan();
    private bool running = true;

    /// <summary>Creates a menu controller with an empty plan.</summary>
    public Options() { }

    /// <summary>Reads one line from the shared console; end of input terminates the application cleanly.</summary>
    /// <remarks>Using one shared line reader avoids Java Scanner buffering and leftover-newline issues.</remarks>
    internal static string ReadInput() => Console.ReadLine() ?? throw new EndOfStreamException();

    /// <summary>Reads a signed 32-bit integer from one line; invalid or overflowing input throws.</summary>
    internal static int ReadInteger() => int.Parse(ReadInput(), NumberStyles.Integer, CultureInfo.InvariantCulture);

    /// <summary>Displays the main menu and dispatches selections until option 5 is chosen.</summary>
    public void HandleOptions()
    {
        while (running)
        {
            Console.WriteLine(Menu);
            bool notValid = true;
            while (notValid)
            {
                try
                {
                    switch (ReadInput())
                    {
                        case "1": CreateWeek(); notValid = false; break;
                        case "2": ResumeWeek(); notValid = false; break;
                        case "3": SaveIntoFile(); notValid = false; break;
                        case "4": LoadFile(); notValid = false; break;
                        case "5": running = false; notValid = false; break;
                        // As in Java, unrecognized main-menu input silently retries.
                    }
                }
                catch (Exception e) when (e is not EndOfStreamException)
                {
                    Console.WriteLine("Error. Please enter a valid number.");
                }
            }
        }
    }

    /// <summary>Reads a week name and chooses uniform or individual daily budgets.</summary>
    public void CreateWeek()
    {
        Console.WriteLine("Let's create a new weekly plan!\nWhat is the name of the week?");
        Week newWeek = new Week();
        newWeek.SetName(ReadInput().Trim());
        Console.WriteLine("Perfect! Let's plan your daily budget for the week.\n    1. Set the same budget for every day\n    2. Set a different budget for each day\nEnter the number that matches your choice:");
        bool notValid = true;
        while (notValid)
        {
            try
            {
                switch (ReadInput())
                {
                    case "1": newWeek.SetWeeklyBudgetSame(currentPlan); notValid = false; break;
                    case "2": newWeek.SetWeeklyBudget(currentPlan); notValid = false; break;
                    default: Console.WriteLine("Please choose 1 or 2."); break;
                }
            }
            catch (Exception e) when (e is not EndOfStreamException)
            {
                Console.WriteLine("Error. Enter a valid number.");
            }
        }
    }

    /// <summary>Prompts for a one-based week and day, then adds to or replaces that day's budget.</summary>
    /// <remarks>An empty plan or invalid selection keeps retrying, matching the source.</remarks>
    public void ResumeWeek()
    {
        bool notDone = true;
        while (notDone)
        {
            try
            {
                currentPlan.DisplayAllWeeks();
                Console.WriteLine("Choose a week. Enter a valid integer number.");
                Week week = currentPlan.GetWeek(ReadInteger() - 1);
                week.DisplayDays();
                Console.WriteLine("Choose a day. Enter a valid integer number.");
                Day day = week.GetDay(ReadInteger() - 1);
                Console.WriteLine(day.GetName() + ":\n     1. Add more budget\n     2. Reset the budget\nEnter a valid integer number.");
                switch (ReadInput().Trim())
                {
                    case "1":
                        Console.WriteLine("How much do you want to add more?");
                        day.AddBudget(ReadInteger());
                        Console.WriteLine("Budget added." + day.GetName() + ": " + day.GetPlannedBudget().StringFormat());
                        notDone = false;
                        break;
                    case "2":
                        Console.WriteLine("Let's reset the budget. How much do you want for the day?");
                        day.SetBudget(ReadInteger());
                        Console.WriteLine("Budget reset." + day.GetName() + ": " + day.GetPlannedBudget().StringFormat());
                        notDone = false;
                        break;
                }
            }
            catch (Exception e) when (e is not EndOfStreamException)
            {
                Console.WriteLine("Error. Enter a valid number.");
            }
        }
    }

    /// <summary>Writes one UTF-8 record per week to the supplied name plus .txt, overwriting any existing file.</summary>
    public void SaveIntoFile()
    {
        Console.WriteLine("Let's save the current plan into a file. \nWhat is the name of this plan?");
        string fileName = ReadInput();
        try
        {
            // Java Files.write uses UTF-8 without a BOM; WriteAllLines also supplies the platform newline.
            File.WriteAllLines(fileName + ".txt", currentPlan.GetWeeks().Select(w => w.FileFormat()), new UTF8Encoding(false));
            Console.WriteLine("The plan successfully saved to " + fileName + ".txt");
        }
        catch (IOException e)
        {
            Console.WriteLine("An error occurred while creating or writing the file.");
            Console.Error.WriteLine(e);
        }
        catch (UnauthorizedAccessException e)
        {
            // .NET represents file access failures separately from IOException.
            Console.WriteLine("An error occurred while creating or writing the file.");
            Console.Error.WriteLine(e);
        }
    }

    /// <summary>Loads the exact filename entered and replaces the current plan only after successful parsing.</summary>
    public void LoadFile()
    {
        Console.WriteLine("What is the name of the file?");
        string fileName = ReadInput();
        try
        {
            // Throw for invalid UTF-8 instead of silently replacing damaged characters.
            string[] rawFile = File.ReadAllLines(fileName, new UTF8Encoding(false, true));
            currentPlan = FileToCurPlan(new List<string>(rawFile));
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while reading the file.");
            Console.Error.WriteLine(e);
        }
    }

    /// <summary>Builds a plan from comma-delimited records, reading the name and seven spent/planned pairs.</summary>
    /// <remarks>Stored total columns are ignored. Malformed records throw; extra columns are ignored.</remarks>
    public Plan FileToCurPlan(List<string> rawFile)
    {
        Plan plan = new Plan();
        foreach (string line in rawFile)
        {
            string[] split = line.Split(',');
            Week week = new Week(split[0]);
            for (int day = 0; day < 7; day++)
                week.SetSpentBudgetDaily(day, split[day + 3]);
            plan.AddWeek(week);
        }
        return plan;
    }
}
