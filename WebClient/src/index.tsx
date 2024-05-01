import './index.scss'

import * as React from 'react'
import { createRoot } from 'react-dom/client'

import { Root } from './root'

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
createRoot(document.querySelector('body > client-root')!).render(<Root />)
