using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Stores a meal.</summary>
public class Meal
{
    private String name;
    private double cost;
    private String Ingredients;
    private String NutritionalInformation;
    private String Description;
    private String Instructions;
    private String Author;
    private String Category;
    private String Time;

    /// <summary>Creates a meal with the specified cost.</summary>
    public Meal(String name, int amount, String ingredients, String nutritionalInformation, String description = "", String instructions = "", String author = "", String category = "", String time = "")
    {
        this.name = name;
        this.cost = amount;
        this.Ingredients = ingredients;
        this.NutritionalInformation = nutritionalInformation;
        this.Description = description;
        this.Instructions = instructions;
        this.Author = author;
        this.Category = category;
        this.Time = time;
    }

    /// <summary>Creates a meal with the specified cost, flooring it to two decimal places.</summary>
    public Meal(String name, double amount, String ingredients, String nutritionalInformation, String description = "", String instructions = "", String author = "", String category = "", String time = "")
    {
        this.name = name;
        this.cost = Math.Floor(amount * 100) / 100;
        this.Ingredients = ingredients;
        this.NutritionalInformation = nutritionalInformation;
        this.Description = description;
        this.Instructions = instructions;
        this.Author = author;
        this.Category = category;
        this.Time = time;
    }

    /// <summary>Sets the stored cost without further rounding.</summary>
    public void SetCost(int amount) => this.cost = amount;

    /// <summary>Sets the meal name.</summary>
    public void SetName(String name) => this.name = name;

    /// <summary>Sets the cost after flooring it to two decimal places.</summary>
    public void SetCost(double amount) => this.cost = Math.Floor(amount * 100) / 100;

    /// <summary>Sets the stored ingredients.</summary>
    public void SetIngredients(String ingredients) => this.Ingredients = ingredients;

    /// <summary>Sets the stored nutritional information.</summary>
    public void SetNutritionalInformation(String nutritionalInformation) => this.NutritionalInformation = nutritionalInformation;

    /// <summary>Sets the stored description.</summary>
    public void SetDescription(String description) => this.Description = description;

    /// <summary>Sets the stored instructions.</summary>
    public void SetInstructions(String instructions) => this.Instructions = instructions;

    /// <summary>Sets the stored author.</summary>
    public void SetAuthor(String author) => this.Author = author;

    /// <summary>Sets the stored category.</summary>
    public void SetCategory(String category) => this.Category = category;

    /// <summary>Sets the stored time.</summary>
    public void SetTime(String time) => this.Time = time;

    /// <summary>Returns the stored cost without further rounding.</summary>
    public double GetCost() => cost;

    /// <summary>Returns the meal name.</summary>
    public String GetName() => name;

    /// <summary>Returns the stored ingredients.</summary>
    public String GetIngredients() => Ingredients;

    /// <summary>Returns the stored nutritional information.</summary>
    public String GetNutritionalInformation() => NutritionalInformation;

    /// <summary>Returns the stored description.</summary>
    public String GetDescription() => Description;

    /// <summary>Returns the stored instructions.</summary>
    public String GetInstructions() => Instructions;

    /// <summary>Returns the stored author.</summary>
    public String GetAuthor() => Author;

    /// <summary>Returns the stored category.</summary>
    public String GetCategory() => Category;

    /// <summary>Returns the stored time.</summary>
    public String GetTime() => Time;

    /// <summary>Reads recipe data from a JSON file and assigns it to this meal.</summary>
    public void ReadData(string filePath)
    {
        string json = File.ReadAllText(filePath);
        RecipeData? recipe = JsonSerializer.Deserialize<RecipeData>(json);

        if (recipe is null || string.IsNullOrWhiteSpace(recipe.Name))
            throw new InvalidDataException("The JSON file does not contain a valid recipe.");

        name = recipe.Name;
        SetCost(recipe.Cost);
        Ingredients = recipe.Ingredients;
        NutritionalInformation = recipe.NutritionalInformation;
        Description = recipe.Description;
        Instructions = recipe.Instructions;
        Author = recipe.Author;
        Category = recipe.Category;
        Time = recipe.Time;
    }

    private sealed class RecipeData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("cost")]
        public double Cost { get; set; }

        [JsonPropertyName("ingredients")]
        public string Ingredients { get; set; } = "";

        [JsonPropertyName("nutritionalInformation")]
        public string NutritionalInformation { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = "";

        [JsonPropertyName("author")]
        public string Author { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("time")]
        public string Time { get; set; } = "";
    }

    /// <summary>Returns culture-independent text, retaining Java's decimal suffix for whole values.</summary>
    /// <remarks>This is not fixed two-decimal currency formatting. Scientific notation can differ from Java.</remarks>
    public string StringFormat()
    {
        string formattedCost = cost.ToString("R", CultureInfo.InvariantCulture);
        return $"Cost: {formattedCost}, Ingredients: {Ingredients}, Nutritional Information: {NutritionalInformation}";
    }
}
