import './index.scss'

import * as React from 'react'
import { createRoot } from 'react-dom/client'

// Bilbliothek konfigurieren.
JMSLib.ReactUi.Pictogram.imageRoot = `content/images/`

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
createRoot(document.querySelector('body > client-root')!).render(<VCRNETClient.Ui.Main />)
