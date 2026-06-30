import type { ReactNode } from 'react'

interface Props {
  title: string
  onClose: () => void
  /** Extra width/max-height classes, e.g. 'max-w-lg' or 'max-w-5xl max-h-[85vh] flex flex-col' */
  sizeClass?: string
  /** Whether the modal has a primary (thick) border */
  primary?: boolean
  children: ReactNode
}

export default function Modal({ title, onClose, sizeClass = 'max-w-lg', primary = false, children }: Props) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ backgroundColor: 'rgba(0,0,0,0.3)' }}>
      <div
        className={`rounded-xl p-6 w-full ${sizeClass}`}
        style={{ backgroundColor: '#ffffff', border: primary ? '2px solid #255876' : '1px solid #d5d8d9' }}
      >
        <div className="flex items-center justify-between mb-5">
          <h3 className="text-sm font-bold uppercase tracking-wide" style={{ color: '#255876' }}>{title}</h3>
          <button onClick={onClose} className="btn-ghost text-xs">✕ Close</button>
        </div>
        {children}
      </div>
    </div>
  )
}
