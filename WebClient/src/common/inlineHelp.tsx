﻿import * as React from 'react'

// Konfiguration zur Anzeige einer eingebetteten Beschreibung.
interface IInlineHelpStatic {
    // Die Überschrift.
    title: string

    // Alle Kindelemente.
    children?: React.ReactNode
}

// Zustand der Anzeige einer Beschreibung.
interface IInlineHelpDynamic {
    // Gesetzt, wenn die Anzeige geöffnet ist.
    open?: boolean
}

// React.Js Komponente zur Anzeige einer eingebetteten Beschreibung.
export class InlineHelp extends React.Component<IInlineHelpStatic, IInlineHelpDynamic> {
    // Erstellt die Oberflächenelement.
    render(): React.JSX.Element {
        const isOpen = this.state && this.state.open

        return (
            <div className='vcrnet-inline-help'>
                <h1 onClick={() => this.setState({ open: !isOpen })}>{this.props.title}</h1>
                {isOpen && <div>{this.props.children}</div>}
            </div>
        )
    }
}
