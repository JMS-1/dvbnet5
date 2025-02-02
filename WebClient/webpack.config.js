const path = require('path')

/** Zum Kopieren von statischen Inhalten in das Bundle. */
const copyStatic = require('copy-webpack-plugin')

/** Die dann hier zu finden sind. */
const publicFolder = path.join(__dirname, 'public')

/** Zum Extrahieren eines CSS Stylesheets in das Bundle. */
const extractCss = require('mini-css-extract-plugin')

/** Nachladen bei Änderungen. */
const ReactRefreshWebpackPlugin = require('@pmmmwh/react-refresh-webpack-plugin')
const ReactRefreshTypeScript = require('react-refresh-typescript').default

/** Benamung von CSS Modulen. */
const cssModules = { auto: true, localIdentName: '[local]-[hash:base64:5]', namedExport: false }

module.exports = (env) => {
    const config = {
        /** Hier wird nur ein Einstieg in die Anwendung verwendet - dynamische (Lazy) Komponenten können Chunks erzeugen. */
        entry: { index: path.join(__dirname, './src/index.tsx') },

        /** Man beachte, dass --env in der package.json explizit gesetzt wird. */
        mode: env.production ? 'production' : 'development',

        module: {
            rules: [
                /** Code Dateien (Typescript). */
                {
                    test: /\.(ts|tsx)$/,
                    use: [
                        {
                            loader: require.resolve('ts-loader'),
                            options: {
                                getCustomTransformers: () => ({
                                    before: [!env.production && ReactRefreshTypeScript()].filter(Boolean),
                                }),
                            },
                        },
                    ],
                },
                /** Styling. */
                {
                    test: /\.s?css$/,
                    use: [
                        { loader: extractCss.loader },
                        { loader: 'css-loader', options: { modules: cssModules, sourceMap: false, url: false } },
                        'sass-loader',
                    ],
                },
                /** SVG Dateien. */
                {
                    test: /\.svg$/,
                    use: [{ loader: 'svg-inline-loader', options: {} }],
                },
            ],
        },

        /** Standardausgabe für die Bundledateien. */
        output: { filename: '[name].js', path: path.join(__dirname, 'dist') },

        /** Kopieren statischer Dateien und Extraktion des Stylings - separat für alle Chunks.  */
        plugins: [
            new copyStatic({ patterns: [{ from: publicFolder }] }),
            new extractCss({ filename: 'index.css' }),
            !env.production && new ReactRefreshWebpackPlugin(),
        ].filter(Boolean),

        /** Aktuell unterstützte Quelldateiarten. */
        resolve: { extensions: ['.ts', '.tsx', '.scss', '.js', '.css', '.svg'] },
    }

    /** Besondere Anpassungen für die Entwicklung.*/
    if (config.mode === 'development') {
        /** webpack-dev-server im Hot Reload Modus aufsetzen - die statischen Dateien natürlich passend zum Bundle. */
        config.devServer = { hot: true, port: 3000, static: { directory: publicFolder } }

        /** Source Maps für das Debuggen mit Breakpoints aktivieren. */
        config.devtool = 'source-map'
    }

    return config
}
