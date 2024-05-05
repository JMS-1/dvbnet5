import * as React from 'react'

import { HelpComponent } from './helpComponent'

import { IPage } from '../../app/pages/page'
import { InternalLink } from '../../lib.react/command/internalLink'

export class EditCurrent extends HelpComponent {
    readonly title = 'Laufende Aufzeichnungen verändern'

    render(page: IPage): JSX.Element {
        return (
            <div>
                Sobald eine Aufzeichnung gestartet wird kennt der VCR.NET Recording Service diese in zweierlei Weise: da
                ist zum einen die ursprüngliche Programmierung der Aufzeichnung, die im Regelfall unverändert bleibt. Es
                gibt aber auch eine zweite Sicht über die gerade laufende
                <InternalLink pict='devices' view={page.application.devicesPage.route} /> Aufzeichnung. In dieser wird
                etwa vermerkt, wenn der Anwender den Endzeitpunkt verschiebt. Manchmal ist es wichtig zu verstehen,
                welche Veränderung den VCR.NET Recording Service beeinflussen.
                <br />
                <br />
                Der einfachste Fall ist es, die Programmierung einer Aufzeichnung zu verändern, ohne die gerade aktive
                Ausführung anzurühren. In dieser Situation gilt folgende einfache Regel: jede Veränderung wird
                ignoriert. Der VCR.NET Recording Service wird die laufende Aufzeichnung wie ursprünglich programmiert
                beenden.
                <br />
                <br />
                Wird die laufende Ausführung verändert, so entfernt der VCR.NET Recording Service die zugehörige
                Aufzeichnung aus der Planungsansicht
                <InternalLink pict='plan' view={page.application.planPage.route} />. Handelt es sich um eine{' '}
                <InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnung</InternalLink>, so erscheint
                diese erst bei der nächsten Wiederholung im Plan. Dies kann verwirrend für den Anwender sein, stört die
                Gesamtplanung im VCR.NET Recording Service allerdings nicht.
            </div>
        )
    }
}
