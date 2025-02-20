﻿import { browserWebCall } from '../lib/http/browser'
import { setWebCallRoot } from '../lib/http/config'

// Normalerweise sind wir das selbst
let serverRoot = document.URL.substring(0, document.URL.indexOf('/', document.URL.indexOf('://') + 3))

// Schauen wir uns mal die Betriebsart an
const query = window.location.search
const debug = query === '?debug'

if (debug) serverRoot = 'http://localhost:5093'

// Der Präfix für den Zugriff auf Geräte und Dateien
const protocolEnd = serverRoot.indexOf('://')
const host = serverRoot.substring(protocolEnd + 3)

const deviceUrl = 'dvbnet5://' + host + '/'

let demuxUrl = 'demux://'

// Der Präfix für alle REST Zugiffe
setWebCallRoot(serverRoot + '/api/')

// Führt eine Web Anfrage aus.
export function doUrlCall<TResponseType, TRequestType>(
    url: string,
    method = 'GET',
    request?: TRequestType
): Promise<TResponseType | undefined> {
    return browserWebCall(url, method, request)
}

// Meldet den Verweis zum Aufruf des DVB.NET / VCR.NET Viewers.
export function getDeviceRoot(): string {
    return deviceUrl
}

export function getDemuxRoot(): string {
    return demuxUrl
}

export function setFtpPort(port: number): void {
    demuxUrl = 'demux://' + host.split(':')[0] + ':' + port + '/'
}

// Meldet den Pfad zum Abspielen einer abgeschlossenen Aufzeichnung (DVB.NET / VCR.NET Viewer muss installiert sein).
export function getFilePlayUrl(path: string): string {
    return `${getDeviceRoot()}play=${encodeURIComponent(path)}`
}
