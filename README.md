# BookShelf

BookShelf is a comprehensive web application for book enthusiasts to discover, track, and discuss books. It provides a platform for users to manage their reading journey, connect with fellow readers through book clubs, and share reviews and recommendations.

## 📋 Features

- **User Management**

  - Registration and authentication
  - User profiles
  - Reading goals and progress tracking

- **Book Management**

  - Browse and search books
  - Add new books to the database
  - Detailed book information with ratings and reviews

- **Reading Tracking**

  - Track currently reading books
  - Mark books as read, want to read, or currently reading
  - Set and monitor reading goals

- **Book Clubs**

  - Create and join book clubs
  - Participate in discussions
  - Manage book club membership

- **Reviews & Ratings**

  - Rate and review books
  - View community ratings and reviews
  - Rating distribution statistics

- **Home Page Dashboard**
  - Top-rated books
  - Personalized recommendations
  - Currently reading status
  - Popular book clubs

## 🛠️ Technology Stack

- **Backend**

  - .NET 8.0/9.0
  - ASP.NET Core MVC
  - Entity Framework Core 9.0
  - PostgreSQL Database

- **Frontend**

  - ASP.NET Core MVC Views
  - Bootstrap
  - jQuery

- **Architecture**

  - Clean Architecture (Domain-driven design)
  - Repository pattern
  - Service-based application layer

- **Deployment & DevOps**
  - Docker containers
  - Docker Compose
  - Health monitoring
  - Containerized development and production environments

## 🏗️ Project Structure

The project follows Clean Architecture principles with a clear separation of concerns:

- **BookShelf.Domain**: Core entities, interfaces, and business logic

  - Entities (Book, User, BookClub, etc.)
  - Repository interfaces
  - Domain-specific enums and constants

- **BookShelf.Application**: Application logic and use cases

  - DTOs (Data Transfer Objects)
  - Service interfaces and implementations
  - Mapping profiles (AutoMapper)

- **BookShelf.Infrastructure**: Data access and external concerns

  - Database context and configurations
  - Repository implementations
  - Migrations
  - External service implementations

- **BookShelf**: Web application (presentation layer)
  - Controllers
  - Views
  - Models
  - Configuration and middleware

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Development Setup

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd BookShelf
   ```

2. Run the application using Docker Compose:

   ```bash
   docker-compose up
   ```

3. Access the application at http://localhost:8080

### Production Deployment

1. Set up environment variables:

   ```bash
   # Create a .env.prod file with your production settings
   POSTGRES_USER=your_username
   POSTGRES_PASSWORD=your_secure_password
   ```

2. Deploy using the production script:

   ```bash
   ./deploy-prod.sh
   ```

3. Access the application at http://your-host

## 🐳 Docker

The application is fully containerized:

- **Development**: Uses `docker-compose.yml` with hot reload capabilities
- **Production**: Uses `docker-compose.prod.yml` with optimized settings and health checks

You can push the Docker images to Docker Hub using:

```bash
./push-to-dockerhub.sh
```

## 📚 Database Schema

The application uses PostgreSQL with the following main entities:

- Users
- Books
- Bookshelves
- Book Clubs
- Discussions
- Reviews
- Reading Progress
- Reading Goals

Database migrations are handled by Entity Framework Core.

## ⚙️ Configuration

The application uses different configuration settings based on the environment:

- **Development**: `appsettings.Development.json`
- **Production**: `appsettings.json` with environment variables

Key configuration includes:

- Database connection strings
- Authentication settings
- Logging configuration

## 🔍 Health Monitoring

The application includes health checks for:

- PostgreSQL database connection
- Application status

Health endpoints are available at:

- Development: http://localhost:8080/health
- Production: http://your-host/health

## 👥 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔮 Future Enhancements

- Mobile application integration
- Social media sharing
- Book recommendation engine improvements
- API for third-party integrations
- Comprehensive notification system
