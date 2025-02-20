﻿import * as React from 'react'

import { HelpComponent } from './helpComponent'
import { ScreenShot } from './screenShot'

import { IPage } from '../../app/pages/page'
import { InternalLink } from '../../lib.react/command/internalLink'

export class Decryption extends HelpComponent {
    readonly title = 'Entschlüsselung'

    render(page: IPage): React.JSX.Element {
        return (
            <div>
                Der VCR.NET Recording Service kann die Fähigkeiten von{' '}
                <InternalLink view={`${page.route};dvbnet`}>DVB.NET</InternalLink> nutzen, um mit Hilfe einer CI/CAM
                Hardware und einer entsprechenden Abonnementen-Karte Quellen zu entschlüsseln und in unverschlüsselter
                Form aufzuzeichnen. Im Allgemeinen sind dabei besondere Einstellungen des DVB.NET Geräteprofils
                notwendig, auf die hier aber nicht eingegangen werden kann.
                <br />
                <br />
                Eine wichtige, wenn auch zurzeit noch irrelevante Einstellung
                <InternalLink pict='admin' view={`${page.application.adminPage.route};devices`} /> ist die Anzahl der
                gleichzeitig entschlüsselbarer Quellen. Irrelevant ist diese Einstellung in der vorliegenden Version, da
                DVB.NET zu jeder Zeit immer nur eine Quelle entschlüsseln kann. Im DVB.NET Geräteprofil wird bei den
                Aufzeichnungseinstellungen ein entsprechender Grenzwert verwaltet, der als Voreinstellung 1 verwendet.
                <ScreenShot description='Gleichzeitige Entschlüsselung' name='faq/decryptlimit' />
                Die Aufzeichnungsplanung berücksichtigt diesen Wert und garantiert bei der Voreinstellung, dass auch in{' '}
                <InternalLink view={`${page.route};parallelrecording`}>parallelen Aufzeichnungen</InternalLink> zu jedem
                Zeitpunkt pro DVB Gerät maximal die festgelegte Anzahl von Quellen entschlüsselt wird. Wird dieser Wert
                auf 0 verändert, so werden Aufzeichnungen von verschlüsselten Quellen automatisch aus der Planung
                entfernt.
                <br />
                <br />
                Es wird empfohlen die Voreinstellung nicht zu verändern. In seltenen Fällen kann es vorkommen, dass eine
                Quelle zwar als verschlüsselte gekennzeichnet ist, dies aber entweder nicht wirklich der Tatsache
                entspricht oder die Verschlüsselung nur zeitweise aktiv ist. Wäre die Anzahl gleichzeitiger
                Entschlüsselung im DVB.NET Geräteprofil 0, so könnte eine solche Quelle grundsätzlich nicht
                aufgezeichnet werden, da sie aus der Planung entfernt wird. Zusätzlich ist zu beachten, dass die Planung
                sich immer an der Liste der Quellen eines DVB.NET Geräteprofils orientiert. Dies wird zu bestimmten
                Zeitpunkten <InternalLink view={`${page.route};psiconfig`}>aktualisiert</InternalLink> und kann
                eventuell falsche Informationen enthalten, wenn die Sendeanstalten fehlerhafte Informationen bezüglich
                der Quellen liefern.
            </div>
        )
    }
}
