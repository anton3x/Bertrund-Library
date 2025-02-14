# 📚 Bertrund

## Project Description

**Bertrund** is an online library system developed with **ASP.NET MVC**, **Bootstrap**, **SignalR**, **SQL Server**, and hosted on **Azure App Service**. The project offers advanced features for managing books and users, with specific access levels for readers, librarians, and administrators. Additionally, it includes modern features like **Google authentication**, an intelligent chatbot powered by **Google Gemini**, **Cloudflare Turnstile** for bot protection, and support for light and dark themes.

---

## 📷 Project Images

![Home Page Light](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/HomePage1Light.png)
[Home Page Dark](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/HomePageDark.png)
![Profile Page Light](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/ProfilePageLight.png)
[Profile Page Dark](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/ProfilePageDark.png)
![Book Catalog Page Light](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/BookCatalogPageLight.png)
[Book Catalog Page Dark](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/BookCatalogPageDark.png)
![Chats Page Light](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/ChatPageLight.png)
[Chats Page Dark](https://github.com/anton3x/Bertrund-Library/blob/388b50c0fdb02b30cb39e70003ae3371477bbfc1/Images/ChatsPageDark.png)


## 🎯 Key Features

### **Users**
#### Readers:
- Request book loans for **15 days**.
- View history of loans, returns, and reservations.

#### Librarians:
- Manage the book catalog.
- Manage the author catalog.
- Manage the category catalog.
- Manage the loan catalog.

#### Administrators:
- Control and manage users.

---

## 🌍 User Experience

- **Multilingual Support**: Interface available in multiple languages.
- **Light/Dark Mode**: Theme switching for visual comfort.

---

## 🔐 Security Features

- **CAPTCHA Cloudflare Turnstile**: Protection against bots.
- **Google Authentication**: Secure login using OAuth 2.0.
- **Intelligent Chatbot**: User support via Google Gemini API.

---

## 🛠️ Technologies Used

- **ASP.NET MVC**: Robust backend for dynamic management.
- **Bootstrap 5**: Modern and responsive design.
- **SQL Server on Azure**: Reliable and scalable database.
- **Cloudflare Turnstile**: Secure and seamless CAPTCHA integration.
- **Google OAuth**: Trusted authentication.
- **Azure App Service**: Scalable and efficient hosting.
- **Google Gemini**: AI platform.
- **SignalR**: Real-time communication for interactive applications.
- **Daily.co**: Integration platform for video and voice calls.

---

1. **Clone the Repository**

   Clone the repository to your local machine:
   ```bash
   git clone https://github.com/anton3x/bertrund.git
   ```
   
2. **Configure the Application**
   
   Update the appsettings.json file with your configurations:
   ```bash
   {
      "ConnectionStrings": {
        "DefaultConnection": "your-default-db-connection"
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "EmprestimoCleanup": {
        "DiasParaExclusao": 3,
        "IntervaloVerificacaoHoras": 1
      },
      "EmailConfiguration": {
        "FromEmail": "noreply@example.com",
        "DisplayName": "System Notifications",
        "SmtpServer": "smtp.gmail.com",
        "Port": 587,
        "Username": "your-email@example.com",
        "Password": "your-email-password"
      },
      "Auth": {
        "Google": {
          "ClientId": "your-google-client-id",
          "ClientSecret": "your-google-client-secret"
        }
      },
      "Turnstile": {
        "SiteKey": "your-turnstile-site-key",
        "SecretKey": "your-turnstile-secret-key"
      },
      "Gemini": {
        "ApiKey": "your-gemini-api-key",
        "Model": "tunedModels/copy-of-copy-of-prompt-tunning-a6dexfock"
      },
      "DailyCo": {
        "ApiKey": "your-dailyco-api-key",
        "DailyApiBaseUrl": "https://api.daily.co/v1/"
      }
    }

   ```
    
3. **Set Up the Development Environment**

   Ensure you have installed:
   + .NET 8.0 SDK or later
   + SQL Server
    
4. **Set Up the Database**
    ```bash
      update-database
    ```
5. **Run the Application**
6. **Access the Application**

   Open your browser and navigate to:
   ```bash
    https://localhost:5001
   ```
