using System;
using System.Globalization;

/// <summary>Stores a meal.</summary>
public class Meal
{
    private double cost;
    private String Ingredients;
    private String NutritionalInformation;

    /// <summary>Creates a meal with the specified cost.</summary>
    public Meal(int amount, String ingredients, String nutritionalInformation)
    {
        this.cost = amount;
        this.Ingredients = ingredients;
        this.NutritionalInformation = nutritionalInformation;
    }

    /// <summary>Creates a meal with the specified cost, flooring it to two decimal places.</summary>
    public Meal(double amount, String ingredients, String nutritionalInformation)
    {
        this.cost = Math.Floor(amount * 100) / 100;
        this.Ingredients = ingredients;
        this.NutritionalInformation = nutritionalInformation;
    }

    /// <summary>Sets the stored cost without further rounding.</summary>
    public void SetCost(int amount) => this.cost = amount;

    /// <summary>Sets the cost after flooring it to two decimal places.</summary>
    public void SetCost(double amount) => this.cost = Math.Floor(amount * 100) / 100;

    /// <summary>Sets the stored ingredients.</summary>
    public void SetIngredients(String ingredients) => this.Ingredients = ingredients;

    /// <summary>Sets the stored nutritional information.</summary>
    public void SetNutritionalInformation(String nutritionalInformation) => this.NutritionalInformation = nutritionalInformation;

    /// <summary>Returns the stored cost without further rounding.</summary>
    public double GetCost() => cost;

    /// <summary>Returns the stored ingredients.</summary>
    public String GetIngredients() => Ingredients;


    /// <summary>Returns the stored nutritional information.</summary>
    public String GetNutritionalInformation() => NutritionalInformation;

    /// <summary>Returns culture-independent text, retaining Java's decimal suffix for whole values.</summary>
    /// <remarks>This is not fixed two-decimal currency formatting. Scientific notation can differ from Java.</remarks>
    public string StringFormat()
    {
        string formattedCost = cost.ToString("R", CultureInfo.InvariantCulture);
        return $"Cost: {formattedCost}, Ingredients: {Ingredients}, Nutritional Information: {NutritionalInformation}";
    }
}
