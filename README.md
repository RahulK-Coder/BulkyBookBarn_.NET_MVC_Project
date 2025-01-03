# BulkyBookBarn - ASP.NET MVC Project

Welcome to **BulkyBookBarn**, a fully functional online bookstore built using **ASP.NET MVC**. This project demonstrates the development of a multi-functional web application with user authentication, role-based access control, and a complete shopping experience.

## Features

- **User Authentication and Authorization**: Login, registration, and role-based access control for admins, users, and employees.
- **Product Management**: Admins can manage books, categories, and authors via a dashboard interface.
- **Shopping Cart and Checkout**: Users can add books to their cart, manage items, and complete the purchase process.
- **Order Management**: Admins can view and manage orders, and users can track their order history.
- **Responsive Design**: The app is fully responsive and works on mobile, tablet, and desktop devices.

## Technologies Used

- **ASP.NET Core MVC**: Framework for building the web application.
- **Entity Framework Core**: ORM for database management.
- **SQL Server**: For data storage.
- **Bootstrap**: Frontend framework for a responsive user interface.
- **Razor Pages**: For dynamic content rendering.
- **SendGrid API**: For email notifications.
- **Stripe API**: For payment processing.

## Screenshots

Home Page
<img width="949" alt="image" src="https://github.com/user-attachments/assets/3d5953b5-2842-453e-8870-8af2d49f40ed">

Cart Page
<img width="958" alt="image" src="https://github.com/user-attachments/assets/c436ffed-76ee-4fcc-85c1-e123e9136637">

Shopping Cart
<img width="956" alt="image" src="https://github.com/user-attachments/assets/457a77fc-43a0-40a8-8c39-d5c415f4b18b">

Order Summary Page
<img width="959" alt="image" src="https://github.com/user-attachments/assets/445b63be-60c7-4ff1-a89d-86044f78bf48">

Register Page
<img width="1275" alt="image" src="https://github.com/user-attachments/assets/7e031f2d-114c-4221-91d0-23df8f60b17e">

Login Page
<img width="1274" alt="image" src="https://github.com/user-attachments/assets/36e5db58-9bd2-4cf4-b359-9a95e329d400">


## Prerequisites

To run the project locally, you need:

- **.NET SDK** (version 5.0 or higher)
- **SQL Server** or **SQL Server Express** for the database
- **Visual Studio** (with ASP.NET Core development workload)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/kundaram/BulkyBookBarn_.NET_MVC_Project.git
   ```
2. Open the project in **Visual Studio**.

3. Install the required NuGet packages:
   ```bash
   dotnet restore
   ```

4. Set up the database:
   - Update the `appsettings.json` file with your SQL Server connection string.
   - Run the migrations to create the database:
     ```bash
     dotnet ef database update
     ```

5. Configure the Stripe API keys for payment processing:
   - In `appsettings.json`, add your **Stripe Publishable Key** and **Secret Key**.

6. Run the project:
   ```bash
   dotnet run
   ```

## Usage

- **Admin**: Manage books, categories, and user roles.
- **Users**: Browse books, add them to the shopping cart, and proceed with the checkout.
- **Orders**: Track your purchase history and view order details.
