import * as React from 'react'

import { HelpComponent } from './helpComponent'

import { IPage } from '../../app/pages/page'
import { ExternalLink } from '../../lib.react/command/externalLink'
import { InternalLink } from '../../lib.react/command/internalLink'

export class DvbNet extends HelpComponent {
    readonly title = 'DVB.NET'

    render(page: IPage): React.JSX.Element {
        return (
            <div>
                Der VCR.NET Recording Service ist ausschließlich der Koordinator von Aufzeichnungen. Die eigentlichen
                Aktivitäten
                <InternalLink pict='devices' view={page.application.devicesPage.route} /> auf den DVB Geräten werden mit
                Hilfe der <ExternalLink url='http://www.psimarron.net/DVBNET/'>DVB.NET Bibliothek</ExternalLink>{' '}
                ausgeführt. Insbesondere muss für jedes DVB Gerät, das der VCR.NET Recording Service nutzen soll, ein{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNET/html/profiles.html'>
                    DVB.NET Geräteprofil
                </ExternalLink>{' '}
                angelegt und korrekt konfiguriert werden. Es wird empfohlen, sich mit den grundsätzlichen Konzepten von
                DVB.NET vertraut zu machen, um zu verstehen, wie der VCR.NET Recording Service auf die vorhandene
                Hardware zugreift. Insbesondere für die{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNET/html/sourcescan.html'>
                    Liste der Quellen und deren Aktualisierung
                </ExternalLink>{' '}
                ist eine korrekte Konfiguration von vitaler Bedeutung.
            </div>
        )
    }
}
