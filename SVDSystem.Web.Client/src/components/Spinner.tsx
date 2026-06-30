export default function Spinner() {
  return (
    <div className="flex items-center justify-center h-40">
      <div className="w-8 h-8 border-4 border-t-transparent rounded-full animate-spin" style={{ borderColor: '#255876', borderTopColor: 'transparent' }} />
    </div>
  )
}
