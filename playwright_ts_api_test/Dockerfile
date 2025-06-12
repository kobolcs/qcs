# Use the official Playwright image with browsers pre-installed
FROM mcr.microsoft.com/playwright:v1.44.0-jammy

# Set the working directory inside the container
WORKDIR /app

# Copy package files and install dependencies
COPY package.json ./
COPY package-lock.json ./
RUN npm ci

# Copy the rest of the project files
COPY . .

# Run the Playwright tests when the container launches
CMD ["npx", "playwright", "test"]