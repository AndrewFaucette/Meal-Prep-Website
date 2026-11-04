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
}