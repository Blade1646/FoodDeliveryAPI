public class User : Person{
    public string login { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string phoneNumber { get; set; } = string.Empty;
    public double bonuses { get; set; }
    public bool status { get; set; }
    public DateTime dob { get; set; }
    public ICollection<Order> Orders { get; set; }  = new List<Order>();

}