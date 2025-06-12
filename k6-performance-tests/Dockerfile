# Use the official k6 image
FROM grafana/k6:latest

# Set the working directory
WORKDIR /home/k6

# Copy the local test script to the container
COPY smoke-test.js .

# Run the test script when the container launches
CMD ["run", "smoke-test.js"]