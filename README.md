# Repo Dashboard & Project Tracker

A full-stack, secure web application built with **.NET 8** and **ASP.NET Core Identity** that allows developers to manage their GitHub repositories, organize project notes, and perform bulk or single repository imports directly from GitHub.

---

## Features

- **Secure Authentication:** Built-in multi-tenant user registration and login using ASP.NET Core Identity and JSON Web Tokens (JWT).
- **GitHub Integration:** Seamlessly import individual repositories (`username/reponame`) or bulk-import all public repositories for a given GitHub username.
- **Custom Project Management:** Assign priority levels (High, Medium, Low), statuses (In Progress, Completed, etc.), and private notes to any repository.
- **Data Isolation:** Fully secured backend controllers ensuring users can only view and edit their own private project notes.
- **Modern UI:** A responsive, sleek dark-mode dashboard styled with Tailwind CSS.

---

## Tech Stack

- **Backend:** C#, .NET 8 Web API, Entity Framework Core, SQLite
- **Auth:** ASP.NET Core Identity, JWT Bearer Authentication
- **Frontend:** HTML5, JavaScript (ES6+), Tailwind CSS
- **External APIs:** GitHub REST API (`HttpClient`)

---

## Getting Started Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.
