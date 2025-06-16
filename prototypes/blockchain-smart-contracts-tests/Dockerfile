FROM node:18-slim
WORKDIR /app
COPY package.json ./
RUN npm install --silent
COPY . .
CMD ["npx", "hardhat", "test"]
