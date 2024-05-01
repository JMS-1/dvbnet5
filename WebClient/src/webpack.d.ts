/** Hilfsdeklaration um mit Typescript SASS Module zu verwenden. */
declare module '*.module.scss' {
    const classes: { readonly [key: string]: string }

    export default classes
}

/** Hilfsdeklaration um mit Typescript SVGs zum importieren. */
declare module '*.svg' {
    const src: string

    export default src
}
