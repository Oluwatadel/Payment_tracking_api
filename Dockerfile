# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

# Copy project file and restore dependencies
COPY ["PaymentTracker.csproj", "./"]
RUN dotnet restore "PaymentTracker.csproj"

# Copy source code and publish
COPY . .
RUN dotnet publish "PaymentTracker.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app from builder
COPY --from=builder /app/publish .

# Expose port (default)
EXPOSE 5000

# Health check — use PORT env var if present (fallback to 5000)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:${PORT:-5000}/health || exit 1

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Let the runtime provide the PORT; fallback to 5000. Start app listening on that port.
ENTRYPOINT ["sh", "-c", "dotnet PaymentTracker.dll --urls http://0.0.0.0:${PORT:-5000}"]
