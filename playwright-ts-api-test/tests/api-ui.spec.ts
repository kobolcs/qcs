import { test, expect } from '@playwright/test'
import { readFileSync, existsSync } from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'
import { PokeDisplayPage } from './pages/PokeDisplayPage'

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const html = readFileSync(path.join(__dirname, 'fixtures/poke-page.html'), 'utf-8')

test.describe('PokeDisplayPage', () => {
    test('shows Pokémon details after search (API intercept, uses POM)', async ({ page }) => {
        // Arrange: Instantiate the Page Object Model
        const pokePage = new PokeDisplayPage(page)

        // Arrange: Set up the API intercept using the POM method
        await pokePage.interceptApi('**/api/pokemon/pikachu', {
            name: 'Pikachu',
            sprite: 'https://img.pokemondb.net/sprites/pikachu.png'
        })

        // Arrange: Load the page content
        await pokePage.loadContent(html)

        // Act: Perform the search using the POM method
        await pokePage.searchForPokemon('pikachu')

        // Assert: Use the POM locators to verify the result
        console.log(await pokePage.nameLocator.textContent())
        await expect(pokePage.nameLocator).toHaveText('Pikachu', { timeout: 10000 })
        await expect(pokePage.spriteLocator).toHaveAttribute('src', 'https://img.pokemondb.net/sprites/pikachu.png')

        // Optionally verify UI visually with a screenshot snapshot when
        // a baseline image is present.
        const baseline = path.join(
            __dirname,
            'api-ui.spec.ts-snapshots',
            `pikachu-search-${process.platform}.png`
        )
        if (existsSync(baseline)) {
            await expect(page).toHaveScreenshot('pikachu-search.png')
        } else {
            console.warn('Skipping screenshot comparison: baseline not found')
        }
    })

    test('shows Not found on API 404', async ({ page }) => {
        const pokePage = new PokeDisplayPage(page)

        // Intercept the API call to return a 404
        await page.route('**/api/pokemon/unknown', route => {
            route.fulfill({ status: 404, body: '' })
        })

        await pokePage.loadContent(html)
        await pokePage.searchForPokemon('unknown')

        console.log(await pokePage.nameLocator.textContent())
        await expect(pokePage.nameLocator).toHaveText('Not found', { timeout: 10000 })
    })
})
