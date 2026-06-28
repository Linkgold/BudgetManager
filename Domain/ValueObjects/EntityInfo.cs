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
            return Name == other.Name && Description == other.Description;
        }

        public override bool Equals(object obj) => Equals(obj as EntityInfo);

        public override int GetHashCode() => HashCode.Combine(Name, Description);

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Description))
                return Name;
            return $"{Name}: {Description}";
        }

        public static bool operator ==(EntityInfo a, EntityInfo b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(EntityInfo a, EntityInfo b)
        {
            return !(a == b);
        }

        public static implicit operator string(EntityInfo info) => info.Name;
        public static explicit operator EntityInfo(string name) => new EntityInfo(name);
    }
}