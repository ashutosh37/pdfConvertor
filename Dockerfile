# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file to leverage Docker layer caching
COPY pdfConvertor.csproj .

# Restore dependencies
RUN dotnet restore pdfConvertor.csproj

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish pdfConvertor.csproj -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4.0 AS final
WORKDIR /home/site/wwwroot

# Install LibreOffice for DOCX conversion
# Use --no-install-recommends to minimize image size and clean up apt cache
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends libreoffice && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set the port Azure Functions expects to listen on
ENV WEBSITES_PORT=8080

# Required for Azure Functions Host. Set this in your deployment environment (e.g., Azure App Settings).
# ENV AzureWebJobsStorage="Your_Azure_Storage_Connection_String"