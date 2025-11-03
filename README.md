# CarParkAPI

**Prerequisites**

.NET SDK 8.0+

**Setup**

In main directory:

* dotnet restore .\CarParkAPI.sln

* dotnet build   .\CarParkAPI.sln

* dotnet run --project .\CarPark.API\CarPark.API.csproj --urls "https://localhost:5001"

Open in browser:
https://localhost:5001/swagger

**Tools and Patterns Used**

* .NET 8.0

* MediatR - CQRS + domain event mediator

* FluentValidation

* xUnit + Moq + FluentAssertions - testing

* Domain-Driven Design (DDD)

* Entity Framework (in-memory)

**Assumptions Made**

* Single Parking Lot

* In-memory Entity Framework storage

* Data resets when the app restarts

* Vehicles are uniquely identified by registration (VehicleReg)

* Configurable via appsettings.json → "Pricing" section

* Always uses the first available (lowest number) space
