# Meal Prep server: Express + MongoDB Atlas

This is a local development starter for your HTML website and C# console application. It provides create, list, read, replace, update, and delete operations for `users`, `weeks`, and `recipes`. Your original project files have not been edited.

## Run it

Install Node.js 24 or later. Open PowerShell in the extracted `meal-prep-server` folder:

```powershell
npm install
Copy-Item .env.example .env
```

`npm install` downloads the two libraries listed in package.json: Express handles HTTP requests; the official MongoDB driver handles Atlas access. Node's built-in crypto module hashes passwords. No Mongoose or extra password library is needed.

Edit `.env` with your Atlas connection string and database name. `.env` is local server configuration and is excluded by `.gitignore`. Never copy it into public/ or commit it.

In Atlas, create an application database user with read/write access to your chosen database and allow the Node machine's IP address. A database user is different from your Atlas website login. URL-encode special characters in the password portion of your connection string. The server creates collections and indexes on startup. Existing duplicate username/email values can prevent the unique indexes from being created; use a fresh development database or resolve duplicate records first.

```powershell
npm start
```

Successful startup prints `Connected to MongoDB. Server ready at http://127.0.0.1:3000`. Open that address in your browser. The Node process stays running and waits for requests. `127.0.0.1` means your own computer, and `3000` is the port identifying this server. Press Ctrl+C to stop it. `npm run dev` restarts automatically after server code changes.

This starter binds to your own computer. It has no login or per-user authorization: any local client that can reach it can manage all records. Password hashing does not supply access control. Add authentication and ownership checks before making it available to other people.

## Endpoints

Substitute `users`, `weeks`, or `recipes` for RESOURCE:

| Method | Address | Meaning |
| --- | --- | --- |
| POST | /api/RESOURCE | Create one document; returns 201 and the new document |
| GET | /api/RESOURCE | List documents; returns `{ "data": [...], "page": 1, "limit": 20 }` |
| GET | /api/RESOURCE/ID | Read one document |
| PUT | /api/RESOURCE/ID | Replace all editable fields; required fields must be supplied |
| PATCH | /api/RESOURCE/ID | Change only supplied top-level fields |
| DELETE | /api/RESOURCE/ID | Delete one document; returns 204 with no body |

An endpoint is a URL where the server performs a particular action. GET retrieves data, POST creates it, PATCH modifies it, PUT replaces it, and DELETE removes it. Request and response data use JSON, the text format both JavaScript and C# understand.

All write requests must use `Content-Type: application/json` and send one object, not an array. IDs are 24-character MongoDB ObjectId strings, not the old numeric C# user IDs. MongoDB generates `_id` on creation. JSON responses encode ObjectIds as strings. Do not send `_id`, passwordHash, createdAt, or updatedAt in request bodies.

List pagination: `/api/weeks?page=1&limit=20` (limit up to 100). Weeks and recipes also support `/api/weeks?userId=YOUR_USER_ID`.

Common errors are JSON objects such as `{ "error": "name is required." }`: 400 for invalid input or ID, 404 for missing documents, 409 for duplicate usernames/emails, 413 for oversized JSON, and 500 for unexpected server/database failures.

## Try the complete CRUD flow

These commands create sample data in the database configured in .env. Use a development database. Run them in a second PowerShell window from this server folder while the server is running.

```powershell
$baseUrl = 'http://127.0.0.1:3000'

# Create one user. Save the response so its generated ID can be reused.
$user = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method Post -ContentType 'application/json' -Body (Get-Content .\examples\user.json -Raw)

# Create a week linked to that user.
$weekBody = Get-Content .\examples\week.json -Raw | ConvertFrom-Json
$weekBody | Add-Member -NotePropertyName userId -NotePropertyValue $user._id
$week = Invoke-RestMethod -Uri "$baseUrl/api/weeks" -Method Post -ContentType 'application/json' -Body ($weekBody | ConvertTo-Json -Depth 10)

# Create a recipe.
$recipe = Invoke-RestMethod -Uri "$baseUrl/api/recipes" -Method Post -ContentType 'application/json' -Body (Get-Content .\examples\recipe.json -Raw)

# Read all weeks, then read just the new week.
Invoke-RestMethod -Uri "$baseUrl/api/weeks"
Invoke-RestMethod -Uri "$baseUrl/api/weeks/$($week._id)"

# Change selected fields with PATCH.
Invoke-RestMethod -Uri "$baseUrl/api/users/$($user._id)" -Method Patch -ContentType 'application/json' -Body '{"username":"alice_updated"}'
Invoke-RestMethod -Uri "$baseUrl/api/weeks/$($week._id)" -Method Patch -ContentType 'application/json' -Body '{"name":"Updated Week 01"}'
Invoke-RestMethod -Uri "$baseUrl/api/recipes/$($recipe._id)" -Method Patch -ContentType 'application/json' -Body '{"cost":7.25}'

# Replace a complete recipe with PUT using the original example.
Invoke-RestMethod -Uri "$baseUrl/api/recipes/$($recipe._id)" -Method Put -ContentType 'application/json' -Body (Get-Content .\examples\recipe.json -Raw)

# Delete ONLY the records created above.
Invoke-RestMethod -Uri "$baseUrl/api/weeks/$($week._id)" -Method Delete
Invoke-RestMethod -Uri "$baseUrl/api/recipes/$($recipe._id)" -Method Delete
Invoke-RestMethod -Uri "$baseUrl/api/users/$($user._id)" -Method Delete
```

Rerunning the create-user request before deleting its existing user returns 409. Use another username/email or delete that test user. Every collection supports the same methods, so replace `weeks` with `users` or `recipes` to read/list those documents too.

## Data shapes and rules

See examples/user.json, examples/week.json, and examples/recipe.json for complete POST bodies.

- Users require `username`, `email`, and `password`. Usernames/emails become lowercase and are unique for API-created records. Passwords must contain 12-128 characters. Passwords are salted and hashed with Node's scrypt before storage. Neither hashes nor passwords are returned. There is no login endpoint in this CRUD starter.
- Weeks require `name` and `days`. `days` must have exactly one of each Monday-Sunday. Each day has planned.amount, spent.amount, and plannedMeal; that meal has name, cost, ingredients, and nutritionalInformation. Meal name is optional for older samples and defaults to an empty string. planComplete defaults to false.
- Recipes require `name`, `cost`, `ingredients`, and `nutritionalInformation`. Optional website fields are description, instructions, author, category, and time. The category is quick, budget, vegetarian, or other. Cost represents the planned cost of the single meal in this starter; keep that basis consistent when copying it into a week.
- Weeks/recipes accept optional `userId` as a string or null. It is an association only: it is not an authorization check or an enforced foreign key. Deleting a user preserves their weeks/recipes and their historical userId references. The API does not cascade deletions or verify that the user exists.
- A planned meal is an embedded snapshot. Editing/deleting a recipe does not change meals already placed in weeks.
- `PATCH` merges top-level fields only. Sending `days` replaces the entire seven-day array; it does not merge individual days. To change one meal, GET the week, modify the desired day, then PATCH `{ "days": [...] }` with all seven days. Unknown fields and nested update operators are rejected.
- Dollar amounts are nonnegative numbers, maximum 1000000, with at most two decimal places. This preserves your current samples and C# double representation. Do exact monetary calculations in integer cents (or redesign storage to Decimal128/C# decimal) before adding totals; this starter does not calculate totals.
- No automatic weekly totals are stored. Your current C# Week.SetSpentTotal bug should be fixed before using its computed results. planComplete means planning is finished, not that spending has finished.
- Older manually imported documents can be read without validation, but new POST/PUT/PATCH input is validated. Old `userName` is shown as username on read; use `username` for all new requests. Existing plaintext passwords are not migrated automatically: recreate demo users through POST or PATCH each user's password. PUT requires a new password because it replaces all editable user data.
- Concurrent writes use last-write-wins behavior. Add version checking before multiple clients edit the same week at the same time.

## Connect HTML and C#

Copy only public website assets into public/: HTML, CSS, images, and browser JavaScript. Do not copy the project root, .git, .env, or C# code there. The placeholder index.html is the landing page. If you copy main.html, open `/main.html` or deliberately make it the index page. Existing forms are not automatically wired to these endpoints.

Browser example when the page is served by this Express server:

```javascript
const response = await fetch('/api/weeks');
if (!response.ok) throw new Error('Could not load weeks.');
const result = await response.json();
console.log(result.data);
```

`fetch()` sends a request; `await` waits without blocking the browser; `response.json()` converts returned JSON text into a JavaScript object. For writes, add method, JSON content type, and `body: JSON.stringify(yourObject)`.

C# example inside an async method:

```csharp
using System.Net.Http.Json;
using System.Text.Json;

// Reuse one HttpClient for the lifetime of the application.
using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:3000") };
var result = await client.GetFromJsonAsync<JsonElement>("/api/weeks");
Console.WriteLine(result.GetProperty("data"));

// Update a selected week's name. Replace this with an actual returned _id.
string weekId = "YOUR_WEEK_ID";
using var response = await client.PatchAsJsonAsync(
    $"/api/weeks/{weekId}", new { name = "Updated from C#" });
response.EnsureSuccessStatusCode();
```

HttpClient sends HTTP requests; the JSON helper methods serialize and deserialize data. C# talks only to Node and does not need Atlas credentials. Use DTO classes with public properties when replacing the JsonElement demonstration with strongly typed week objects. Your existing classes use private fields/getter methods and need explicit mapping. The new meal name and MongoDB string IDs also need corresponding DTO properties.

## Files and learning notes

- server.js reads configuration, opens one shared MongoClient, creates indexes, and starts listening only after the database connects.
- app.js registers routes. `app.post('/api/users', async (req, res) => ...)` runs when a POST request arrives: req holds input and res sends output. Express 5 forwards rejected async handlers to the error handler. `$set` changes selected MongoDB fields; replaceOne replaces a complete document; deleteOne deletes one match.
- validation.js defines allowed input fields. Constructing clean documents prevents a client from injecting MongoDB operators through the body.
- public/ contains only files visitors should be able to download.
- examples/ contains request bodies, not MongoDB import arrays. Submit one object per POST.

## Verification

Run `npm test` for HTTP route and validation tests using an in-memory test double. These check successful CRUD and important failure responses; they do not connect to MongoDB. Real Atlas connectivity, indexes, and persistence require the startup check and example CRUD flow against your own development database.

Documentation: [Express](https://expressjs.com/en/5x/api/), [MongoDB insert operations](https://www.mongodb.com/docs/drivers/node/current/crud/insert/), [Atlas connections](https://www.mongodb.com/docs/atlas/connect-to-database-deployment/), [Node scrypt](https://nodejs.org/api/crypto.html#cryptoscryptpassword-salt-keylen-options-callback).
