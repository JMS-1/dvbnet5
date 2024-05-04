import { getGuideInfo, IGuideInfoContract } from './IGuideInfoContract'

// Verwaltet die Zusammenfassung der Daten der Programmzeitschrift für einzelne Geräte
export class GuideInfoCache {
    private static promises: { [device: string]: Promise<IGuideInfoContract> } = {}

    static getPromise(profileName: string): Promise<IGuideInfoContract> {
        // Eventuell haben wir das schon einmal gemacht
        let promise = GuideInfoCache.promises[profileName]
        if (!promise)
            GuideInfoCache.promises[profileName] = promise = new Promise<IGuideInfoContract>((success) =>
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                getGuideInfo(profileName).then((i) => success(i!))
            )

        // Verwaltung melden.
        return promise
    }
}
