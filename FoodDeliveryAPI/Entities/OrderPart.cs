public class OrderPart : BaseEntity{
    public int OrderId { get; set; }  // Внешний ключ к заказу
    public Order? Order { get; set; }  // Навигационное свойство к заказу

    public int DishId { get; set; }  // Внешний ключ к блюду
    public Dish? Dish { get; set; }  // Навигационное свойство к блюду

    public int Quantity { get; set; }
}