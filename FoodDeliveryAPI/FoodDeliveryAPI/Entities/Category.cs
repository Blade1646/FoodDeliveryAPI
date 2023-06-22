public class Category : BaseEntity{
    public string name { get; set; } = string.Empty;
    public double bonusCoefficient { get; set; }
    public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}