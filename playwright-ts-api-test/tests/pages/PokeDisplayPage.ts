import type { Page, Locator } from '@playwright/test'

/**
 * Represents the Page Object for the Pokémon display page.
 * This class encapsulates the locators and actions related to the page,
 * making the tests cleaner and more maintainable.
 */
export class PokeDisplayPage {
    // Page instance from Playwright
    readonly page: Page

    // Locators for the elements on the page
    readonly nameLocator: Locator
    readonly spriteLocator: Locator
    readonly searchButton: Locator
    readonly searchInput: Locator

    /**
     * @param page - Playwright Page instance
     */
    constructor(page: Page) {
        this.page = page

        // Initialize locators using Playwright's recommended practices
        this.nameLocator = page.locator('#name')
        this.spriteLocator = page.locator('#sprite')
        this.searchInput = page.locator('#search')
        this.searchButton = page.locator('#search-btn')
    }

    /**
     * Intercepts a network request to the Pokémon API and provides a mock response.
     * @param url - The URL glob to intercept.
     * @param response - The JSON object to return as the response body.
     */
    async interceptApi(url: string, response: object): Promise<void> {
        await this.page.route(url, route => {
            route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify(response)
            })
        })
    }

    /**
     * Loads the provided HTML content directly into the page.
     * @param html - The HTML string to load.
     */
    async loadContent(html: string): Promise<void> {
        await this.page.setContent(html)
    }

    /**
     * Simulates a user searching for a Pokémon.
     * @param name - The name of the Pokémon to search for.
     */
    async searchForPokemon(name: string): Promise<void> {
        await this.searchInput.fill(name)
        await this.searchButton.click()
    }
}
