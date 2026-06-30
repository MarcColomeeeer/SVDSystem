interface Props { label: string; value: string }

export default function ReadonlyField({ label, value }: Props) {
  return (
    <div>
      <label className="block text-xs font-medium mb-1" style={{ color: '#6b7f88' }}>{label}</label>
      <p className="px-3 py-2 rounded-md text-sm font-mono break-all" style={{ backgroundColor: '#f0f2f2', color: '#1a2e3b', border: '1px solid #d5d8d9' }}>{value}</p>
    </div>
  )
}
