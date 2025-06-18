export default [
  {
    files: ['**/*.ts'],
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
