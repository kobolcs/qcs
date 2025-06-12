# Stage 1: Build the application using Maven
FROM maven:3.9-eclipse-temurin-17 AS build
WORKDIR /app
COPY pom.xml .
COPY src ./src
# Build the project and create the executable JAR
RUN mvn clean install -DskipTests

# Stage 2: Create the final lightweight runtime image
FROM eclipse-temurin:17-jre-jammy
WORKDIR /app
# Copy the executable JAR from the build stage
COPY --from=build /app/target/*.jar app.jar
# Expose the port the application runs on
EXPOSE 8080
# Set the entrypoint to run the application
ENTRYPOINT ["java", "-jar", "app.jar"]