import * as React from 'react'

import { IPage } from '../app/pages/page'
import { InternalLink } from '../lib.react/command/internalLink'
import { IEmpty } from '../lib.react/reactUi'

// Konfiguration für den Verweis auf eine Hilfeseite.
interface IHelpLinkStatic {
    // Der Navigationsbereich, aus dem der der Aufruf erfolgt.
    page: IPage

    // Der Name der Hilfeseite.
    topic: string
}

// React.Js Komponente zur Anzeige eines Verweis auf eine Hilfeseite.
export class HelpLink extends React.Component<IHelpLinkStatic, IEmpty> {
    // Oberflächenelemente anlegen.
    render(): React.JSX.Element {
        return (
            <span className='vcrnet-helpLink'>
                <InternalLink pict='info' view={`${this.props.page.application.helpPage.route};${this.props.topic}`} />
            </span>
        )
    }
}
