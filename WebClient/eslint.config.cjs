const js = require('@eslint/js')

const { FlatCompat } = require('@eslint/eslintrc')

const compat = new FlatCompat({
    allConfig: js.configs.all,
    baseDirectory: __dirname,
    recommendedConfig: js.configs.recommended,
})

module.exports = [
    ...compat.extends('@insynergie'),
    {
        rules: {
            '@typescript-eslint/explicit-function-return-type': 'off',
            'react/no-unescaped-entities': 'off',
            'unused-imports/no-unused-imports-ts': 'warn',
            'unused-imports/no-unused-vars-ts': 'off',
        },
    },
]
