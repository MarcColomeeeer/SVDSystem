interface Props {
  search: string
  onSearchChange: (value: string) => void
  filterUser: string
  onFilterUserChange: (value: string) => void
  users: string[]
  searchPlaceholder?: string
}

/**
 * Reusable search input + "All users" selector bar used in list sections.
 */
export default function SearchUserFilter({
  search,
  onSearchChange,
  filterUser,
  onFilterUserChange,
  users,
  searchPlaceholder = 'Search…',
}: Props) {
  return (
    <div className="flex gap-2">
      <input
        className="input flex-1 text-xs"
        placeholder={searchPlaceholder}
        value={search}
        onChange={e => onSearchChange(e.target.value)}
      />
      <select
        className="input w-44 text-xs"
        value={filterUser}
        onChange={e => onFilterUserChange(e.target.value)}
      >
        <option value="">All users</option>
        {users.map(u => <option key={u} value={u}>{u}</option>)}
      </select>
    </div>
  )
}
