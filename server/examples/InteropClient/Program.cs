using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

// Run against a DEVELOPMENT database. Creates and then deletes one recipe.
using var client = new HttpClient {
    BaseAddress = new Uri(args.Length > 0 ? args[0] : "http://127.0.0.1:3000"),
    Timeout = TimeSpan.FromSeconds(20)
};
var input = new {
    name = "C# interoperability check", cost = 6.75m,
    ingredients = "Rice, beans", nutritionalInformation = "Demo only",
    instructions = "Created by C#", category = "budget"
};
RecipeDto created = await Read<RecipeDto>(await client.PostAsJsonAsync("/api/recipes", input));
string path = $"/api/recipes/{Uri.EscapeDataString(created.Id)}";
Console.WriteLine($"POST: created {created.Id}");
try {
    RecipeDto fetched = await Read<RecipeDto>(await client.GetAsync(path));
    if (fetched.Name != input.name || fetched.Cost != input.cost)
        throw new Exception("GET did not return the values written by POST.");
    Console.WriteLine($"GET: {fetched.Name}, ${fetched.Cost}");

    PageDto page = await Read<PageDto>(await client.GetAsync("/api/recipes?page=1&limit=5"));
    Console.WriteLine($"LIST: {page.Data.Count} records on page {page.Page}");

    // C# processes database-fetched data, then sends JSON (including text) back.
    decimal processedCost = fetched.Cost + 1.00m;
    RecipeDto patched = await Read<RecipeDto>(await client.PatchAsJsonAsync(path, new {
        cost = processedCost, instructions = "Processed in C#: added $1 to fetched cost."
    }));
    RecipeDto reread = await Read<RecipeDto>(await client.GetAsync(path));
    if (patched.Cost != processedCost || reread.Cost != processedCost ||
        !reread.Instructions.StartsWith("Processed in C#"))
        throw new Exception("PATCH result did not round-trip through GET.");
    Console.WriteLine($"PATCH + GET: processed cost ${reread.Cost}; text saved");

    RecipeDto replaced = await Read<RecipeDto>(await client.PutAsJsonAsync(path, input));
    if (replaced.Cost != input.cost) throw new Exception("PUT did not restore the cost.");
    Console.WriteLine("PUT: replaced all editable recipe fields");
} finally {
    // Cleanup only the record created by this invocation, even after a failed check.
    using var deleted = await client.DeleteAsync(path);
    await Check(deleted);
    if (deleted.StatusCode != HttpStatusCode.NoContent)
        throw new Exception("Expected DELETE to return 204.");
    using var missing = await client.GetAsync(path);
    if (missing.StatusCode != HttpStatusCode.NotFound)
        throw new Exception("Expected GET after DELETE to return 404.");
    Console.WriteLine("DELETE: 204; subsequent GET: 404");
}
Console.WriteLine("C# / Node HTTP and JSON checks passed.");

static async Task Check(HttpResponseMessage response) {
    if (!response.IsSuccessStatusCode)
        throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
}

static async Task<T> Read<T>(HttpResponseMessage response) {
    using (response) {
        await Check(response);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new Exception("Expected a JSON response body.");
    }
}

// DTOs are public properties for HTTP JSON, separate from private domain fields.
sealed class RecipeDto {
    [JsonPropertyName("_id")] public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Cost { get; set; }
    public string Instructions { get; set; } = "";
}
sealed class PageDto {
    public List<RecipeDto> Data { get; set; } = [];
    public int Page { get; set; }
    public int Limit { get; set; }
}
