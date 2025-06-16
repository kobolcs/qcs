import { test, expect } from "@playwright/test";
import { readFileSync } from "fs";
import path from "path";
import { PokeDisplayPage } from "./pages/PokeDisplayPage";

const html = readFileSync(path.join(__dirname, "fixtures/poke-page.html"), "utf-8");

test.describe("PokeDisplayPage", () => {
    test("shows Pokémon details after search (API intercept, uses POM)", async ({ page }) => {
        // Arrange: Instantiate the Page Object Model
        const pokePage = new PokeDisplayPage(page);

        // Arrange: Set up the API intercept using the POM method
        await pokePage.interceptApi("**/api/pokemon/pikachu", {
            name: "Pikachu",
            sprite: "https://img.pokemondb.net/sprites/pikachu.png"
        });

        // Arrange: Load the page content
        await pokePage.loadContent(html);

        // Act: Perform the search using the POM method
        await pokePage.searchForPokemon("pikachu");

        // Assert: Use the POM locators to verify the result
        await expect(pokePage.nameLocator).toHaveText("Pikachu");
        await expect(pokePage.spriteLocator).toHaveAttribute("src", "https://img.pokemondb.net/sprites/pikachu.png");

        // Take a screenshot and compare it to a stored snapshot.
        await expect(page).toHaveScreenshot("pikachu-search.png");
    });

    test("shows Not found on API 404", async ({ page }) => {
        const pokePage = new PokeDisplayPage(page);

        // Intercept the API call to return a 404
        await page.route("**/api/pokemon/unknown", route => {
            route.fulfill({ status: 404, body: "" });
        });

        await pokePage.loadContent(html);
        await pokePage.searchForPokemon("unknown");

        await expect(pokePage.nameLocator).toHaveText("Not found");
    });
});