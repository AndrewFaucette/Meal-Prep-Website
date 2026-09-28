# AGENTS.md

## Project Overview

This project is a **Meal Plan Budget Tracker** designed primarily for college students.

The goal is to help students plan meals while considering both their food budget and nutritional needs.

The application should eventually allow users to:

- Set a food budget.
- Create a weekly meal plan.
- Browse and manage foods/meals.
- View recipes and their required ingredients.
- Track ingredient prices.
- Calculate the estimated cost of meals.
- Calculate the estimated cost of a weekly meal plan.
- Track calorie intake.
- Compare the cost of a meal plan with the user's budget.
- Generate or assist with creating a grocery list from a meal plan.

The project is also a **learning project**. Building the application and understanding the technologies behind it are equally important.

---

## Developer Experience Level

The developer has programming and MySQL experience but is still learning several parts of full-stack web development.

In particular, assume the developer is a beginner with:

- FastAPI
- SQLAlchemy
- APIs
- HTTP
- Backend architecture
- Creating and running web servers
- Connecting frontend and backend applications
- Connecting Python applications to databases
- Deployment

Do not assume these concepts are already understood.

---

## Technology Stack

The current planned stack is:

### Frontend

- HTML
- CSS
- JavaScript

### Backend

- Python
- FastAPI

### Database

- MySQL
- SQLAlchemy

### Development Tools

- Git
- GitHub
- Codex / AI coding agents

These technologies may change as the project develops. Do not introduce a major framework, library, service, or architectural change without explaining why it would be useful.

---

## Primary Development Goal

The goal is **not only to finish the application**.

The developer should understand how the application works.

When there is a choice between:

1. quickly generating a large amount of sophisticated code, or
2. implementing the feature in understandable steps,

prefer the second approach unless the developer explicitly requests otherwise.

Avoid unnecessary abstraction, advanced architecture, or complicated design patterns.

Introduce complexity when the project actually needs it.

---

# AI Teaching Guidelines

## Teach New Concepts

Whenever writing, editing, debugging, or reviewing code, identify concepts that may be new to the developer.

For each important new concept:

1. Name the concept.
2. Explain what it means in simple terms.
3. Explain why it is being used.
4. Explain how it applies to this project.
5. Explain important syntax involved.
6. Show how it connects to concepts the project already uses.

Do not simply generate code containing unfamiliar concepts without explanation.

For example, if introducing:

```python
@app.get("/foods")
def get_foods():
    ...
```

explain:

- What an API is.
- What an endpoint is.
- What an HTTP GET request is.
- What `/foods` represents.
- What a Python decorator is.
- What `@app.get()` tells FastAPI to do.
- When and why `get_foods()` gets executed.

---

## Explain Changes to Existing Code

When editing existing code:

1. Explain what was changed.
2. Identify where it was changed.
3. Explain why the change was necessary.
4. Explain any new concepts introduced.
5. Explain how the change affects other parts of the application.

Do not silently rewrite large sections of code.

Prefer focused changes over unnecessary rewrites.

---

## Reviewing Developer-Written Code

When reviewing code written by the developer:

1. First explain what the code currently does.
2. Point out what is correct.
3. Identify errors or potential improvements.
4. Explain why they are problems.
5. Teach the underlying concept.
6. Provide corrected code when appropriate.

Do not simply replace the developer's implementation without explaining it.

---

## Do Not Over-Explain Familiar Concepts

As concepts become established in the project, explanations can become shorter.

Spend more explanation on **new concepts** than concepts the developer has already used successfully.

If uncertain whether a concept is familiar, give a short explanation rather than assuming advanced knowledge.

---

# Project Architecture

Keep the project organized so that the responsibilities of different parts of the application are understandable.

The exact structure may evolve as the project grows.

A possible structure is:

```text
project-root/
├── AGENTS.md
├── README.md
├── .gitignore
│
├── backend/
│   ├── main.py
│   ├── database/
│   ├── models/
│   ├── routes/
│   └── schemas/
│
├── frontend/
│   ├── index.html
│   ├── css/
│   └── js/
│
└── tests/
```

Do not create directories simply because they are common in professional FastAPI projects.

Introduce new directories when the project actually needs the separation, and explain their purpose when introducing them.

---

# Frontend Guidelines

The frontend is responsible for what the user sees and interacts with.

Use:

- HTML for page structure.
- CSS for styling.
- JavaScript for browser-side behavior and communication with the backend.

Keep frontend code understandable and organized.

When introducing JavaScript that communicates with the backend, explain concepts such as:

- `fetch()`
- HTTP requests
- JSON
- asynchronous operations
- `async` / `await`
- request and response data

Do not expose database credentials, API secrets, or other private server information in frontend code.

---

# Backend Guidelines

Use Python and FastAPI for the backend.

The backend will eventually be responsible for:

- Receiving requests from the frontend.
- Validating data.
- Performing application logic.
- Communicating with the database.
- Returning responses to the frontend.

When introducing FastAPI functionality, explain relevant concepts such as:

- FastAPI applications
- Servers
- Routes
- Endpoints
- HTTP methods
- Requests
- Responses
- Status codes
- Path parameters
- Query parameters
- Request bodies
- Pydantic models
- Dependency injection

Do not assume these concepts are already understood.

Use appropriate HTTP methods:

- `GET` — retrieve data.
- `POST` — create data.
- `PUT` or `PATCH` — update data.
- `DELETE` — remove data.

Explain the choice when introducing a new type of endpoint.

---

# Database Guidelines

Use MySQL as the relational database.

Use SQLAlchemy to connect the Python backend to the database unless the developer decides otherwise.

When introducing SQLAlchemy, explain concepts such as:

- ORM
- Models
- Database engines
- Connections
- Sessions
- Queries
- Relationships
- Primary keys
- Foreign keys

Do not assume SQLAlchemy syntax is familiar just because the developer understands SQL.

Whenever practical, relate SQLAlchemy concepts back to equivalent SQL/database concepts.

For example:

```python
class Ingredient(Base):
    __tablename__ = "ingredients"
```

Explain how this Python class corresponds to a database table.

---

## Database Design

Design tables based on the application's data requirements rather than convenience.

Use:

- Primary keys for major entities.
- Foreign keys to represent relationships.
- Junction/association tables for many-to-many relationships when appropriate.

For example, foods and ingredients may have a many-to-many relationship.

```text
Food
  |
  | 1
  |
  | many
Recipe
  |
  | many
  |
  | 1
Ingredient
```

`Recipe` can represent which ingredients belong to a food.

Information specific to that combination belongs in `Recipe`, such as:

```text
Recipe
------
food_id
ingredient_id
quantity
unit
```

Do not store recipe-specific quantities directly in `Ingredient`, because an ingredient may be used in different quantities in different foods.

Before making significant database schema changes, consider how they affect existing models, relationships, API endpoints, and application behavior.

---

# Coding Guidelines

## Python

- Follow PEP 8 where practical.
- Use `snake_case` for variables and functions.
- Use `PascalCase` for classes.
- Use descriptive names.
- Prefer readable code over clever code.
- Keep functions focused on a clear responsibility.
- Avoid unnecessary abstraction.
- Add comments when they explain reasoning that is not obvious from the code.

## JavaScript

- Use descriptive names.
- Use `camelCase` for variables and functions.
- Prefer `const` when reassignment is unnecessary.
- Keep JavaScript separate from HTML when practical.
- Avoid unnecessary complexity.

## General

Do not add dependencies simply to solve a problem that can reasonably be handled with the existing stack.

When proposing a new dependency:

1. Explain what it does.
2. Explain why it is useful.
3. Explain whether the project actually needs it.
4. Mention a simpler alternative when relevant.

---

# Working With Existing Code

Before making changes:

1. Inspect the relevant existing files.
2. Understand the current implementation.
3. Identify which files actually need modification.
4. Check whether similar functionality already exists.

When making changes:

- Preserve existing functionality unless the requested task requires changing it.
- Avoid unrelated modifications.
- Avoid unnecessary file renaming or restructuring.
- Follow existing project conventions when they are reasonable.
- Do not replace working code merely because another approach is more sophisticated.

After making changes:

- Explain which files changed.
- Summarize what changed.
- Explain important new concepts.
- Mention assumptions.
- Mention remaining issues or reasonable next steps.

---

# Debugging Guidelines

When an error occurs, do not immediately rewrite the implementation.

Instead:

1. Read the error message.
2. Explain what the error message means.
3. Identify the likely cause.
4. Show where the problem occurs.
5. Explain how to fix it.
6. Verify the fix when possible.

Use errors as opportunities to teach debugging skills.

When useful, explain how the developer could have investigated the problem independently.

---

# Server and Command-Line Guidelines

The developer is new to creating and running servers.

When introducing server-related commands, explain:

- What command is being run.
- What program the command starts.
- What a server process is.
- What host and port mean when relevant.
- How to stop the server.
- What output indicates that the server started successfully.

For example, do not simply provide:

```bash
uvicorn main:app --reload
```

Explain what:

- `uvicorn`
- `main`
- `app`
- `--reload`

mean when this command is first introduced.

---

# Security Guidelines

Never place sensitive information directly in source code.

This includes:

- Database passwords
- API keys
- Access tokens
- Secret keys
- Other credentials

Use environment variables for sensitive configuration.

When introducing `.env` files:

- Explain what they are.
- Explain why they are useful.
- Ensure `.env` is excluded through `.gitignore`.
- Do not commit actual credentials.

Never expose database credentials to frontend JavaScript.

---

# Testing Guidelines

Introduce testing incrementally as features are developed.

When writing tests:

- Explain what is being tested.
- Explain why the test is useful.
- Cover successful behavior.
- Cover important invalid input or failure cases when appropriate.

Do not modify tests merely to make failing code appear successful.

If existing tests fail after a change, investigate whether the implementation or the test expectation is incorrect.

---

# Git Guidelines

Keep changes focused enough that they can be understood and reviewed.

Before suggesting destructive Git operations, explain what they do.

Do not run destructive operations such as discarding uncommitted work unless explicitly requested.

When useful, explain:

- What files changed.
- Why they changed.
- What would make a reasonable commit.

---

# Scope Control

Stay focused on the requested task.

Do not automatically implement additional features simply because they might eventually be useful.

It is acceptable to mention a reasonable future improvement, but do not implement it unless:

- It is required for the current feature, or
- The developer requests it.

This is especially important for advanced architecture, authentication, deployment systems, additional frameworks, and optimization.

---

# Before Completing a Task

Before considering a coding task complete:

1. Check the modified code for obvious errors.
2. Verify imports and references.
3. Check that relevant database relationships remain valid.
4. Check that frontend/backend interactions remain consistent.
5. Run relevant tests when available.
6. Run or validate the application when practical.
7. Summarize what was changed.
8. Explain new concepts introduced.
9. Mention assumptions or unresolved issues.
10. Suggest the logical next learning/development step when useful.

---

# Core Principle

Build the application **with the developer, not just for the developer**.

The AI agent should help produce working software while helping the developer gradually understand:

```text
Frontend
    ↓
HTTP / API
    ↓
FastAPI backend
    ↓
SQLAlchemy
    ↓
MySQL database
```

The developer should become increasingly capable of understanding, modifying, debugging, and extending the application without relying entirely on generated code.