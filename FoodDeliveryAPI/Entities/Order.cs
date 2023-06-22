public class Order : BaseEntity{
    
    public int UserId { get; set; }  // Внешний ключ к пользователю
    public User? User { get; set; }  // Навигационное свойство к пользователю

    public int? CourierId { get; set; }  // Внешний ключ к курьеру
    public Courier? Courier { get; set; }  // Навигационное свойство к курьеру
    public double sum {get; set; } 
    public double bonusesAmount {get; set; }
    public string startAddress { get; set; } = string.Empty;
    public string endAddress { get; set; } = string.Empty;
    public bool status { get; set; }
    public DateTime startTime { get; set; }
    public DateTime? endTime { get; set; }
    public ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();

}