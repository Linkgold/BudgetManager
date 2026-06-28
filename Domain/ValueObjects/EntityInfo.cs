namespace Domain.ValueObjects
{
    public class EntityInfo : IEquatable<EntityInfo>
    {
        public string Name { get; }
        public string? Description { get; }

        public EntityInfo(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));

            if (name.Length < 2) throw new ArgumentException("Name must have at least 2 characters", nameof(name));

            if (name.Length > 50) throw new ArgumentException("Name cannot exceed 50 characters", nameof(name));

            if (description != null && description.Length > 200) throw new ArgumentException("Description cannot exceed 200 characters", nameof(description));

            Name = name.Trim();
            Description = description?.Trim();
        }

        public bool Equals(EntityInfo other)
        {
            if (other is null) return false;
            //return Name == other.Name && Description == other.Description;

            // Comparación case-insensitive usando StringComparison.OrdinalIgnoreCase
            return string.Equals(Name ?? "", other.Name ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(Description ?? "", other.Description ?? "", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj) => Equals(obj as EntityInfo);

        public override int GetHashCode()
        {
            // Usar StringComparer.OrdinalIgnoreCase para consistencia
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return HashCode.Combine
            (
                comparer.GetHashCode(Name ?? string.Empty),
                comparer.GetHashCode(Description ?? string.Empty)
            );
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
                return Name;
            return $"{Name}: {Description}";
        }

        public static bool operator ==(EntityInfo a, EntityInfo b) => Equals(a, b);

        public static bool operator !=(EntityInfo a, EntityInfo b) => !Equals(a, b);

        public static implicit operator string(EntityInfo info) => info.Name;
        public static explicit operator EntityInfo(string name) => new EntityInfo(name);
    }
}