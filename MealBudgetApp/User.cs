using System.Text.Json;
using System.Text.Json.Serialization;

public class User
{
    private string username;
    private string password;
    private string email;
    private int id;
    private List<Day> days = new List<Day>();

    /// <summary>Creates a user with the specified username and password.</summary>
    public User(string username, string password, string email, int id)
    {
        this.username = username;
        this.password = password;
        this.email = email;
        this.id = id;
    }

    public User()
    {
        this.username = "";
        this.password = "";
        this.email = "";
        this.id = 0;
    }    

    /// <summary>Returns the stored username.</summary>
    public string GetUsername() => username;

    /// <summary>Returns the stored password.</summary>
    public string GetPassword() => password;

    /// <summary>Returns the stored email.</summary>
    public string GetEmail() => email;

    /// <summary>Returns the stored ID.</summary>
    public int GetId() => id;

    /// <summary>Returns the stored day at the specified index.</summary>
    public Day GetDay(int index) => days[index];

    /// <summary>Sets the password.</summary>
    public void SetPassword(string password) { this.password = password; }

    /// <summary>Sets the email.</summary>
    public void SetEmail(string email) { this.email = email; }

    /// <summary>Sets the ID.</summary>
    public void SetId(int id) { this.id = id; }

    /// <summary>Adds a new day to the user at the specified index.</summary>
    public void AddDay(String name) { days.Add(new Day(name)); }

    /// <summary>Reads user data from a json file.</summary>
    /// Use: user.ReadData("test.json"); - loads the first user in the file
    public void ReadData(string filePath)
    {
        UserData userData = ReadUserData(filePath, null);
        AssignData(userData);
    }

    /// <summary>Reads the user with the specified ID from a json file.</summary>
    /// Use: user.ReadData("test.json", 7); - loads the user with ID 7
    public void ReadData(string filePath, int userId)
    {
        UserData userData = ReadUserData(filePath, userId);
        AssignData(userData);
    }

    private static UserData ReadUserData(string filePath, int? userId)
    {
        string json = File.ReadAllText(filePath);
        List<UserData>? users = JsonSerializer.Deserialize<List<UserData>>(json);

        if (users is null || users.Count == 0)
        {
            throw new InvalidDataException("The JSON file does not contain any users.");
        }

        if (userId is null)
        {
            return users[0];
        }

        UserData? matchingUser = users.Find(user => user.UserId == userId.Value);
        return matchingUser ?? throw new InvalidDataException($"No user with ID {userId.Value} was found.");
    }

    private void AssignData(UserData userData)
    {
        username = userData.UserName;
        password = userData.Password;
        email = userData.Email;
        id = userData.UserId;
    }

    private sealed class UserData
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    }
}
