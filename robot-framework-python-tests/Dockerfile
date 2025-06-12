# Use an official Python runtime as a parent image
FROM python:3.10-slim

# Set the working directory in the container
WORKDIR /app

# Copy the requirements file into the container
COPY requirements.txt .

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Install Robot Framework's browser dependencies
RUN rfbrowser init

# Copy the rest of the application's code
COPY . .

# Run the tests in parallel when the container starts
CMD ["pabot", "--processes", "4", "--outputdir", "reports/", "tests"]