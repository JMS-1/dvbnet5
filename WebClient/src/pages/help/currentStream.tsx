import * as React from 'react'
import { IPage } from '../../app/pages/page'
import { InternalLink } from '../../lib.react/command/internalLink'
import { HelpComponent } from './helpComponent'
import { ScreenShot } from './screenShot'

export class CurrentStream extends HelpComponent {
    readonly title = 'Laufende Aufzeichnungen verändern'

    render(page: IPage): JSX.Element {
        return (
            <div>
                Die Aktivitäten
                <InternalLink view={page.application.devicesPage.route} pict='devices' /> auf einem DVB Gerät können in
                einem gewissen Rahmen verändert werden. Handelt es sich um eine{' '}
                <InternalLink view={`${page.route};tasks`}>Sonderaufgabe</InternalLink> wie der Aktualisierung der{' '}
                <InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</InternalLink>
                , so ist das vorzeitige Beenden ebenso möglich wie eine Veränderung der Laufzeit. Grundsätzlich wird
                allerdings empfohlen, Sonderaufgaben nicht in der konfigurierten Ausführung zu behindern - maximal kann
                ein vorzeitiger Abbruch Sinn machen, wenn es äußere Umstände erforderlich machen.
                <ScreenShot description='Laufzeit verändern' name='FAQ/endchange' />
                Interessanter ist es, wenn der der VCR.NET Recording Service eine DVB Hardware nutzt um Aufzeichnungen
                auszuführen. Je nach konkreter Situation können dies auch{' '}
                <InternalLink view={`${page.route};parallelrecording`}>mehrere Aufzeichnungen</InternalLink> sein, die
                parallel ausgeführt werden, wobei diese allerdings separat in der Aktivitätenliste aufgeführt werden.
                Hier sind nun folgende Veränderungen möglich:
                <br />
                <ul>
                    <li>Eine Aufzeichnung kann vorzeitig beendet werden.</li>
                    <li>
                        Das Ende einer Aufzeichnung kann verschoben werden - eine Verkürzung ist nur soweit möglich,
                        dass der Endzeitpunkt immer noch in der Zukunft liegt.
                    </li>
                </ul>
                Es empfiehlt sich grundsätzlich, Manipulationen an laufenden Aufzeichnungen mit Bedacht vorzunehmen. Bei
                Veränderungen des Endzeitpunktes kann es etwa dazu kommen, dass die betroffenen Aufzeichnungen nicht
                mehr in der Aufzeichnungsplanung erscheinen
                <InternalLink view={page.application.planPage.route} pict='plan' />.
            </div>
        )
    }
}
