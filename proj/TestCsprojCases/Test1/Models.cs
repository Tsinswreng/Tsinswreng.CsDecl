namespace Test1.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public UserModel() {
        Id = 0;
        Name = "";
        Email = "";
    }

    public UserModel(int id, string name, string email) {
        Id = id;
        Name = name;
        Email = email;
    }

    public void UpdateProfile(string newName, string newEmail) {
        Name = newName;
        Email = newEmail;
    }

    public bool IsValid() {
        return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email);
    }

    public event Action<UserModel> ProfileUpdated;
}

public record ProductRecord(string Name, decimal Price)
{
    public void ApplyDiscount(decimal percentage) {
        // Implementation
    }

    public decimal GetDiscountedPrice() {
        return Price;
    }
}

public struct PointStruct
{
    public int X { get; set; }
    public int Y { get; set; }

    public PointStruct(int x, int y) {
        X = x;
        Y = y;
    }

    public double DistanceTo(PointStruct other) {
        return Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
    }
}
