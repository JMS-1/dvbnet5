import * as React from 'react'

import { IEmpty } from '../reactUi'

// Konfiguration eines externen Verweises.
interface IExternalLink {
    // Der volle Verweis.
    url: string

    // Gesetzt, wenn eine Anzeige im aktuellen Fenster erfolgen soll - bevorzugt erfolgt die Darstellung in einem neuen Fenster.
    sameWindow?: boolean

    // Kindelemente.
    children?: React.ReactNode
}

// React.Js Komponente zur Anzeige eines externen Verweises.
export class ExternalLink extends React.Component<IExternalLink, IEmpty> {
    // Erstellt die Oberflächenelemente.
    render(): React.JSX.Element {
        return (
            <a
                className='jmslib-externalLink'
                href={this.props.url}
                rel='noreferrer'
                target={this.props.sameWindow ? undefined : '_blank'}
            >
                {this.props.children}
            </a>
        )
    }
}
