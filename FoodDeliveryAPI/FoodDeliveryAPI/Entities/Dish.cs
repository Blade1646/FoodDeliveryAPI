public class Dish : BaseEntity{
    public string name { get; set; } = string.Empty;
    public double price { get; set; }
    public string description { get; set; } = "Інформація відсутня";
    public int CategoryId { get; set; }  // Внешний ключ к категории
    public Category? Category { get; set; }  // Навигационное свойство к категории
    public string image { get; set; } = string.Empty;

    public ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
}