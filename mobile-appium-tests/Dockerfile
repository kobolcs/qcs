FROM ghcr.io/reactivecircus/android-emulator:latest

# Configure the emulator default parameters
ENV API_LEVEL=29 \
    TARGET=google_apis \
    ARCH=x86_64 \
    EMULATOR_OPTIONS="-no-window -no-audio"

# Install Python and Node.js
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        python3 python3-pip nodejs npm && \
    rm -rf /var/lib/apt/lists/*

# Install Python dependencies
WORKDIR /tests
COPY requirements.txt requirements.txt
RUN pip3 install --no-cache-dir -r requirements.txt

# Install Appium server and wait-on utility
RUN npm install -g appium wait-on

# Copy the rest of the test suite
COPY . .

# Launch the emulator, start Appium and execute the tests
CMD bash -c "start-emulator >/tmp/emulator.log 2>&1 &\
    adb wait-for-device && \
    appium > appium.log 2>&1 &\
    wait-on http-get://127.0.0.1:4723/status && \
    pytest --junitxml=report.xml"
