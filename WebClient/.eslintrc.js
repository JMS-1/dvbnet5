module.exports = {
    extends: '@insynergie',
    parserOptions: {
        tsconfigRootDir: __dirname,
    },
    rules: {
        'unused-imports/no-unused-imports-ts': 'warn',
        'unused-imports/no-unused-vars-ts': 'off',
    },
}
