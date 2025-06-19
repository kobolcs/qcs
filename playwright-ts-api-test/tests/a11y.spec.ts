import { test, expect } from '@playwright/test'
import { readFileSync } from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'
import AxeBuilder from '@axe-core/playwright' // Import AxeBuilder

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const html = readFileSync(path.join(__dirname, 'fixtures/poke-page.html'), 'utf-8')

test.describe('Accessibility', () => {
    test('page should have no critical accessibility violations', async ({ page }) => {
        await page.setContent(html)

        // Analyze the page with axe
        const accessibilityScanResults = await new AxeBuilder({ page })
            .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']) // Configure rules
            .analyze()

        // Assert that there are no violations
        expect(accessibilityScanResults.violations).toEqual([])
    })
})
