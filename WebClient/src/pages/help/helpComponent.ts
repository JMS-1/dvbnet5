import * as React from 'react'

import { IHelpComponent } from '../../app/pages/help'
import { IPage } from '../../app/pages/page'

// React.Js Komponente zur Implementierung von Hilfeseiten.
export abstract class HelpComponent implements IHelpComponent {
    // Die Überschrift der Hilfeseite.
    abstract readonly title: string

    // Erzeugt die Oberflächenelemente der Hilfeseite.
    abstract render(page: IPage): React.JSX.Element
}
