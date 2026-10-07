# Budget Planner — C# conversion

The six source files are `Budget.cs`, `Day.cs`, `Main.cs`, `Options.cs`, `Plan.cs`, and `Week.cs`. Every constructor and method has XML documentation. The project has no third-party dependencies.

## Run

Install the .NET 10 SDK, open a terminal in this folder, then run:

```text
dotnet run --project BudgetPlanner.csproj
```

Choose a menu option and enter each response on its own line. Saving appends `.txt` to the entered name and overwrites an existing file. Loading expects the complete filename, including the extension. Relative paths refer to the process's current working directory.

## Conversion choices

- `Main.java` becomes `Main.cs` containing `Program.Main(string[] args)`. C# does not allow a method named `Main` inside a class also named `Main`. The unused Java scanner is removed.
- Java lists become `List<T>`: `add`, `get`, `size`, and `indexOf` become `Add`, indexing, `Count`, and `IndexOf`. The alias map becomes a read-only dictionary interface. Existing mutable Budget and list references remain shared.
- `Console.ReadLine` replaces all Scanner instances. One response per line avoids competing scanners, pending newline problems, and repeated reads of an unconsumed invalid token. Week names can now contain spaces. End of input exits cleanly. Numeric prompts still expect signed 32-bit integers.
- Method names generally remain recognizable. `handleOptions` becomes `HandleOptions`, and `FiletoCurPlan` becomes `FileToCurPlan`. The public typo `GetSepntTotal` is retained.
- String comparisons use explicit ordinal value equality, with ordinal case-insensitive matching for aliases and y/n. Java's `==` string checks are translated to value comparisons and identified below; object identity in Plan's newline logic remains `ReferenceEquals`.
- Budgets retain `double` and `Math.Floor(value * 100) / 100`, rather than switching to decimal or rounding. Negative values floor downward; addition/subtraction do not re-round the stored total. Floating-point artifacts and negative amounts remain possible.
- Formatting uses invariant round-trip double text and adds `.0` for whole values, reflecting the source's `Double.toString` intent. It does not format currency to two decimal places. Scientific-notation thresholds, exponent spelling, and some edge-case shortest representations can differ from Java. Parsing daily file amounts uses invariant culture and `NumberStyles.Float`; extreme values and unusual Java numeric forms (such as hexadecimal floating-point literals) may behave differently.
- File I/O uses `File.WriteAllLines`/`File.ReadAllLines` with explicit UTF-8; output has no BOM. Invalid UTF-8 is rejected. .NET's reader can recognize a BOM. Saving uses LINQ `Select` in place of the Java stream. Access-denied exceptions are handled separately because .NET distinguishes them from `IOException`.
- The confirmation loop is shared by a private helper; the two original public planning operations remain separate and keep their original update semantics.

## Source bugs and suspicious logic

These are documented, not silently repaired:

1. `Week.SetSpentTotal` adds each day's spending to **planned**, never updates **spent**, and does not reset anything. `GetSepntTotal` therefore returns zero and can mutate the cached planned amount on each call. `GetTotalBudget` later resets planned. Saved daily spending survives, but the saved/displayed weekly spending total is wrong.
2. `SetSpentDaily` adds spending rather than setting it. Parsing a record into an already populated week therefore accumulates spending; ordinary loading constructs fresh weeks.
3. `SetWeeklyBudget` uses `AddBudget`. Rejecting a proposal or retrying after a partial input error retains prior additions. The uniform-budget operation instead replaces amounts.
4. `Status` originally compares string references against `"0.00"`; C# now compares values. However, zero is formatted as `"0.0"`, so an unplanned week still reports `Completed`. The completion flag is not restored when loading.
5. `Plan(Week)` adds the week before checking its name, so it matches itself and always renames it to `Week 1`. The conversion's value comparison does not change that outcome.
6. Resuming an empty plan repeatedly asks for an impossible selection; there is no back/cancel option. Unknown main-menu options silently retry. The menu has no spending-entry action, consistent with the Java README's future-work list.
7. Records are simple comma/slash-delimited text, not escaped CSV. Names containing commas or newlines cannot round-trip safely. Loading ignores the saved total columns, assumes seven day records, throws on missing/bad fields, and ignores extra fields. No validation or format redesign was added.
8. The Saturday alias list repeats `st`; unknown day names silently leave the name empty. Plan serialization's reference-based newline check can omit separators if the same Week object appears more than once. These behaviors remain.

## Verification

Built with .NET SDK 10.0.401: zero warnings and zero errors, with XML documentation generation enabled.

Automated checks covered negative flooring, invariant formatting, case-insensitive aliases, shared Budget references, daily spending/budget serialization, malformed input records, and the retained total/status/constructor quirks. Console checks covered creation, invalid-number retry, adding/resetting budgets, UTF-8 save/load/save, repeated daily planning, normal exit, and end of input. These checks verify the C# conversion and key intended/preserved behavior; a complete Java-versus-C# differential test was not performed.
