import { clsx } from 'clsx'
import * as React from 'react'

import styles from './root.module.scss'

interface IRootProps {
    className?: string
}

export const Root: React.FC<IRootProps> = (props) => {
    return <div className={clsx(styles.root, props.className)}>[Moin]</div>
}
