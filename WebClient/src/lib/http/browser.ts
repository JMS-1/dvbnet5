import { IHttpErrorInformation } from './common'
import { currentWebCallId, nextWebCallId, webCallRoot } from './config'

// Verwendet den XHMLHttpRequest des Browser zur Durchführung eines HTTP Aufrufs.
export function browserWebCall<TResponseType, TRequestType>(
    url: string,
    method = 'GET',
    request?: TRequestType
): Promise<TResponseType | undefined> {
    // Eindeutige Nummer für den nächsten HTTP Aufruf ermitteln - tatsächlich arbeiten wir hier in 2er Schritten, aber das tut nicht zur Sache.
    const nextId = nextWebCallId()

    // Aynchronen Aufruf aufsetzen.
    return new Promise<TResponseType | undefined>((success, failure) => {
        // Aufruf an eine absolute URL erkennen.
        const raw = url.substring(0, 7) === 'http://'

        // HTTP Aufruf anlegen.
        const xhr = new XMLHttpRequest()

        // HTTP Antwort abwarten.
        xhr.addEventListener('load', () => {
            // Sicherstellen, dass die Antwort überhaupt noch interessiert.
            if (currentWebCallId() != nextId) return

            // Ergebnis auswerten.
            if (xhr.status < 400)
                if (xhr.status === 204)
                    // Antwort wandeln und melden.
                    success(undefined)
                else if (raw) success(xhr.responseText as unknown as TResponseType)
                else success(JSON.parse(xhr.responseText))
            else {
                // Fehler auswerten.
                let errorInfo

                try {
                    errorInfo = JSON.parse(xhr.responseText)
                } catch (e) {
                    errorInfo = e.message
                }

                // Fehler melden - falls es jemanden interessiert.
                failure(<IHttpErrorInformation>{
                    details: errorInfo.MessageDetails,
                    message: errorInfo.ExceptionMessage || errorInfo.Message,
                })
            }
        })

        // Im Falle einer relativen URL den Bezugspunkt nach Angabe des Clients anpassen.
        if (!raw) url = webCallRoot + url

        // Endpunkt einrichten.
        xhr.open(method, url)
        xhr.timeout = 60000

        // Antwortformat anmelden.
        if (!raw) xhr.setRequestHeader('accept', 'application/json')

        // Aufruf absetzen.
        if (request === undefined) {
            // Aufruf ohne Eingangsparameter.
            xhr.send()
        } else {
            // Als Eingangsparameter kennen wir nur JSON.
            xhr.setRequestHeader('content-type', 'application/json')

            // Eingangsparameter wandeln und übertragen.
            xhr.send(JSON.stringify(request))
        }
    })
}
