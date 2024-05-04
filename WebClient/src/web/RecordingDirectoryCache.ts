import { getRecordingDirectories } from './IEditJobContract'

// Verwaltet die Aufzeichnungsverzeichnisse
export class RecordingDirectoryCache {
    // Die zwischengespeicherten Verzeichnisse
    private static promise: Promise<string[]> | null

    // Vergisst alles, was wir wissen
    static reset(): void {
        RecordingDirectoryCache.promise = null
    }

    // Ruft die Verzeichnisse ab
    static getPromise(): Promise<string[]> {
        // Erstmalig laden
        if (!RecordingDirectoryCache.promise) {
            // Verwaltung erzeugen.
            RecordingDirectoryCache.promise = new Promise<string[]>((success) => {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                getRecordingDirectories().then((data) => success(data!))
            })
        }

        // Verwaltung melden.
        return RecordingDirectoryCache.promise
    }
}
