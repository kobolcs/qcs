export default [
  {
    files: ['**/*.{ts,js}'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module'
    },
    rules: {
      semi: ['error', 'never'],
      quotes: ['error', 'single']
    }
  }
]
