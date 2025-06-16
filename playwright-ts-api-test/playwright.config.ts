import { defineConfig } from "@playwright/test";

export default defineConfig({
    testDir: "./tests",
    timeout: 30000,
    retries: 1,
    reporter: [ ["html", { outputFolder: "playwright-report" }] ],
    use: {
        headless: true,
        screenshot: "only-on-failure"
    }
});
