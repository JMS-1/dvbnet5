module.exports = {
    extends: '@insynergie',
    parserOptions: {
        tsconfigRootDir: __dirname,
    },
    rules: {
        '@typescript-eslint/explicit-function-return-type': 'off',
        'unused-imports/no-unused-imports-ts': 'warn',
        'unused-imports/no-unused-vars-ts': 'off',
    },
}
