var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DeliveryDb>(options =>{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    // Дополнительные настройки JSON, если необходимо
};
var app = builder.Build();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDb>();
    db.Database.EnsureCreated();

app.MapGet("/", () => "Hello, world!");

#region USERS
// GET всех пользователей
app.MapGet("/users", async (DeliveryDb db) => await db.users.ToListAsync());

// GET пользователя по айди
app.MapGet("/users/id/{id}", async (int id, DeliveryDb db) => 
    await db.users.FirstOrDefaultAsync(u => u.id == id) is User user 
    ? Results.Ok(user)
    : Results.NotFound());

// GET пользователя по паролю и логину
app.MapGet("/users/logining/{login}/{password}", async (string login, string password, DeliveryDb db) => 
    await db.users.FirstOrDefaultAsync(u => (u.login == login && u.password == password)) is User user 
    ? Results.Ok(user)
    : Results.NotFound());

// GET проверка существует ли пользователь с таким номером телефона
app.MapGet("/users/phoneNumber/{phoneNumber}/{id}", async (string phoneNumber, int id, DeliveryDb db) => 
    await db.users.FirstOrDefaultAsync(u => (u.phoneNumber == phoneNumber && u.id != id)) is User user 
    ? Results.Ok(true)
    : Results.Ok(false));

// GET проверка существует ли пользователь с таким логином
app.MapGet("/users/login/{login}", async (string login, DeliveryDb db) => 
    await db.users.FirstOrDefaultAsync(u => u.login == login) is User user 
    ? Results.Ok(true)
    : Results.Ok(false));

// POST нового пользователя
app.MapPost("/users", async (User user, DeliveryDb db) => {
    db.users.Add(user);
    await db.SaveChangesAsync();
    return user.id;
});

// PUT уже созданного пользователя
app.MapPut("/users", async (User user, DeliveryDb db) => {
    var userFromDb = await db.users.FindAsync(new object[] { user.id });
    if (userFromDb == null){
        return Results.NotFound();
    }
    userFromDb.password = user.password;
    userFromDb.name = user.name;
    userFromDb.surname = user.surname;
    userFromDb.phoneNumber = user.phoneNumber;
    userFromDb.bonuses = user.bonuses;
    userFromDb.status = user.status;
    userFromDb.dob = user.dob;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
// Добавить бонусы пользователю
app.MapPut("/users/id/{id}", async (int id, User user, DeliveryDb db) => {
    var userFromDb = await db.users.FindAsync(new object[] { id });
    if (userFromDb == null){
        return Results.NotFound();
    }
    userFromDb.bonuses += user.bonuses;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
// DELETE пользователя по айди
app.MapDelete("/users/id/{id}", async (int id, DeliveryDb db) =>{
var userFromDb = await db.users.FindAsync(new object[] { id });
    if (userFromDb == null){
        return Results.NotFound();
    }
    db.users.Remove(userFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region COURIERS
// GET всех курьеров
app.MapGet("/couriers", async (DeliveryDb db) => await db.couriers.ToListAsync());

// GET курьера по айди
app.MapGet("/couriers/id/{id}", async (int id, DeliveryDb db) => 
    await db.couriers.FirstOrDefaultAsync(c => c.id == id) is Courier courier 
    ? Results.Ok(courier)
    : Results.NotFound());

// GET курьера по паролю и логину
app.MapGet("/couriers/logining/{login}/{password}", async (string login, string password, DeliveryDb db) => 
    await db.couriers.FirstOrDefaultAsync(c => (c.login == login && c.password == password)) is Courier couriers 
    ? Results.Ok(couriers)
    : Results.NotFound());

// GET проверка существует ли курьер с таким номером телефона
app.MapGet("/couriers/phoneNumber/{phoneNumber}/{id}", async (string phoneNumber, int id, DeliveryDb db) => 
    await db.couriers.FirstOrDefaultAsync(c => (c.phoneNumber == phoneNumber && c.id != id)) is Courier courier 
    ? Results.Ok(true)
    : Results.NotFound(false));

// GET проверка существует ли курьер с таким логином
app.MapGet("/couriers/login/{login}", async (string login, DeliveryDb db) => 
    await db.couriers.FirstOrDefaultAsync(c => c.login == login) is Courier courier 
    ? Results.Ok(true)
    : Results.NotFound(false));

// POST нового курьера
app.MapPost("/couriers", async (Courier courier, DeliveryDb db) => {
    db.couriers.Add(courier);
    await db.SaveChangesAsync();
    return Results.Created($"/couriers/{courier.id}", courier);
});

// PUT уже созданного курьера
app.MapPut("/couriers", async (Courier courier, DeliveryDb db) => {
    var courierFromDb = await db.couriers.FindAsync(new object[] { courier.id });
    if (courierFromDb == null){
        return Results.NotFound();
    }
    courierFromDb.password = courier.password;
    courierFromDb.name = courier.name;
    courierFromDb.surname = courier.surname;
    courierFromDb.phoneNumber = courier.phoneNumber;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE курьера по айди
app.MapDelete("/couriers/id/{id}", async (int id, DeliveryDb db) =>{
var courierFromDb = await db.couriers.FindAsync(new object[] { id });
    if (courierFromDb == null){
        return Results.NotFound();
    }
    db.couriers.Remove(courierFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region CATEGORIES
// GET всех категорий
app.MapGet("/categories", async (DeliveryDb db) => await db.categories.ToListAsync());
app.MapGet("/categoriesWithDishes", async (DeliveryDb db) => 
    {
        var categories = await db.categories.ToListAsync();
        var settings = new JsonSerializerSettings
        {
           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 64 //
        };
        foreach (var category in categories){
            category.Dishes = await db.dishes.Where(d => d.CategoryId == category.id).ToListAsync();
        }
        var json = JsonConvert.SerializeObject(categories, settings);
        return Results.Ok(json);
    }
);
// GET категории по айди
app.MapGet("/categories/id/{id}", async (int id, DeliveryDb db) => 
    await db.categories.FirstOrDefaultAsync(c => c.id == id) is Category category 
    ? Results.Ok(category)
    : Results.NotFound());

// GET категории по названию
app.MapGet("/categories/name/{name}", async (string name, DeliveryDb db) => 
    await db.categories.FirstOrDefaultAsync(c => c.name == name) is Category category 
    ? Results.Ok(category.id)
    : Results.NotFound(-1));

// POST новой категории
app.MapPost("/categories", async (Category category, DeliveryDb db) => {
    db.categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/categories/{category.id}", category);
});

// PUT уже созданной категории
app.MapPut("/categories", async (Category category, DeliveryDb db) => {
    var categoryFromDb = await db.categories.FindAsync(new object[] { category.id });
    if (categoryFromDb == null){
        return Results.NotFound();
    }
    categoryFromDb.name = category.name;
    categoryFromDb.bonusCoefficient = category.bonusCoefficient;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE курьера по айди
app.MapDelete("/categories/id/{id}", async (int id, DeliveryDb db) =>{
var categoryFromDb = await db.categories.FindAsync(new object[] { id });
    if (categoryFromDb == null){
        return Results.NotFound();
    }
    db.categories.Remove(categoryFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region ORDERS
// GET все заказы
app.MapGet("/orders", async (DeliveryDb db) => await db.orders.ToListAsync());

// GET заказ по айди
app.MapGet("/orders/id/{id}", async (int id, DeliveryDb db) => 
    await db.orders.FirstOrDefaultAsync(o => o.id == id) is Order order 
    ? Results.Ok(order)
    : Results.NotFound());

// GET заказы по айди пользователя
app.MapGet("/orders/user/{id}", async (int id, DeliveryDb db) => 
    {
        var orders = await db.orders.Where(o => o.UserId == id).ToListAsync();
        return Results.Ok(orders);
 });

// GET заказы по айди курьера
app.MapGet("/orders/courier/{id}", async (int id, DeliveryDb db) => 
    {var orders = await db.orders.Where(o => o.CourierId == id).ToListAsync(); 
    return Results.Ok(orders);
 });

// POST нового заказа
app.MapPost("/orders", async (Order order, DeliveryDb db) => {
    db.orders.Add(order);
    await db.SaveChangesAsync();
    return order.id;
});
// PUT заказа
app.MapPut("/orders/idEndTime/{id}", async (int id, Order order, DeliveryDb db) => {
    var orderFromDb = await db.orders.FindAsync(new object[] { id });
    if (orderFromDb == null){
        return "Order not found";
    }
    orderFromDb.endTime = order.endTime;
    await db.SaveChangesAsync();
    return "Order found";
});
app.MapPut("/orders/idStatus/{id}", async (int id, DeliveryDb db) => {
    var orderFromDb = await db.orders.FindAsync(new object[] { id });
    if (orderFromDb == null){
        return "Order not found";
    }
    orderFromDb.status = true;
    await db.SaveChangesAsync();
    return "Order found";
});
// PUT заказа
app.MapPut("/orders/courierApprove/id/{id}", async (int id, CourierApprove ca, DeliveryDb db) => {
    var orderFromDb = await db.orders.FindAsync(new object[] { id });
    if (orderFromDb == null){
        return Results.NotFound();
    }
    orderFromDb.CourierId = ca.CourierId;
    orderFromDb.endTime = ca.endTime;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
// DELETE заказ по айди
app.MapDelete("/orders/id/{id}", async (int id, DeliveryDb db) =>{
var orderFromDb = await db.orders.FindAsync(new object[] { id });
    if (orderFromDb == null){
        return Results.NotFound();
    }
    db.orders.Remove(orderFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region DISHES
// GET все блюда
app.MapGet("/dishes", async (DeliveryDb db) => await db.dishes.ToListAsync());

// GET блюдо по айди
app.MapGet("/dishes/id/{id}", async (int id, DeliveryDb db) => 
    await db.dishes.FirstOrDefaultAsync(d => d.id == id) is Dish dish 
    ? Results.Ok(dish)
    : Results.NotFound());

// GET блюдо по имени
app.MapGet("/dishes/name/{name}", async (string name, DeliveryDb db) => 
    await db.dishes.FirstOrDefaultAsync(d => d.name == name) is Dish dish 
    ? Results.Ok(dish)
    : Results.NotFound());

// GET цену блюда по имени
app.MapGet("/dishes/priceName/{name}", async (string name, DeliveryDb db) => 
    await db.dishes.FirstOrDefaultAsync(d => d.name == name) is Dish dish 
    ? Results.Ok(dish.price)
    : Results.NotFound());

// GET цену блюда по айди
app.MapGet("/dishes/priceId/{id}", async (int id, DeliveryDb db) => 
    await db.dishes.FirstOrDefaultAsync(d => d.id == id) is Dish dish 
    ? Results.Ok(dish.price)
    : Results.NotFound());

// GET блюдо по имени
app.MapGet("/dishes/nameCheck/{name}", async (string name, DeliveryDb db) => 
    await db.dishes.FirstOrDefaultAsync(d => d.name == name) is Dish dish 
    ? Results.Ok(true)
    : Results.NotFound(false));

// POST новое блюдо
app.MapPost("/dishes", async (Dish dish, DeliveryDb db) => {
    db.dishes.Add(dish);
    await db.SaveChangesAsync();
    return Results.Created($"/dishes/{dish.id}", dish);
});

// PUT уже созданного пользователя
app.MapPut("/dishes", async (Dish dish, DeliveryDb db) => {
    var dishFromDb = await db.dishes.FindAsync(new object[] { dish.id });
    if (dishFromDb == null){
        return Results.NotFound();
    }
    dishFromDb.name = dish.name;
    dishFromDb.price = dish.price;
    dishFromDb.CategoryId = dish.CategoryId;
    dishFromDb.image = dish.image;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE заказ по айди
app.MapDelete("/dishes/id/{id}", async (int id, DeliveryDb db) =>{
var dishFromDb = await db.dishes.FindAsync(new object[] { id });
    if (dishFromDb == null){
        return Results.NotFound();
    }
    db.dishes.Remove(dishFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region ORDERPARTS
// GET все ордер парты
app.MapGet("/orderParts", async (DeliveryDb db) => await db.orderParts.ToListAsync());

// GET ордер парт по айди
app.MapGet("/orderParts/id/{id}", async (int id, DeliveryDb db) => 
    await db.orderParts.FirstOrDefaultAsync(o => o.id == id) is OrderPart orderPart 
    ? Results.Ok(orderPart)
    : Results.NotFound());

// GET ордер парт по айди блюда
app.MapGet("/orderParts/dishId/{id}", async (int id, DeliveryDb db) => 
    {var orderParts = await db.orderParts.Where(o => o.DishId == id).ToListAsync(); 
    return Results.Ok(orderParts);
 });

// GET ордер парт по айди ордера
app.MapGet("/orderParts/orderId/{id}", async (int id, DeliveryDb db) => 
    {var orderParts = await db.orderParts.Where(o => o.OrderId == id).ToListAsync(); 
    return Results.Ok(orderParts);
 });

// POST новый ордер парт
app.MapPost("/orderParts", async (OrderPart orderPart, DeliveryDb db) => {
    db.orderParts.Add(orderPart);
    await db.SaveChangesAsync();
    return Results.Created($"/orderParts/{orderPart.id}", orderPart.OrderId);
});
// POST новый ордер парт
app.MapPost("/orderParts/all/{orderId}", async (int orderId, [FromBody] OrderList list, DeliveryDb db) =>
{
    for (int i = 0; i < list.products.Count; i++)
    {
        OrderPart orderPart = new OrderPart();
        orderPart.OrderId = orderId;
        orderPart.Quantity = list.amount[i];
        orderPart.DishId = await db.dishes.Where(d => d.name == list.products[i]).Select(d => d.id).FirstOrDefaultAsync();
        db.orderParts.Add(orderPart);
    }
    await db.SaveChangesAsync();
    
    return Results.Ok();
});
// DELETE заказ по айди
app.MapDelete("/orderParts/id/{id}", async (int id, DeliveryDb db) =>{
var orderPartFromDb = await db.orderParts.FindAsync(new object[] { id });
    if (orderPartFromDb == null){
        return Results.NotFound();
    }
    db.orderParts.Remove(orderPartFromDb);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion
app.Run();

public class DeliveryDb : DbContext{
    public DeliveryDb(DbContextOptions<DeliveryDb> options) : base(options) {}
    public DbSet<User> users => Set<User>();
    public DbSet<Order> orders => Set<Order>();
    public DbSet<Courier> couriers => Set<Courier>();
    public DbSet<Category> categories => Set<Category>();
    public DbSet<Dish> dishes => Set<Dish>();
    public DbSet<OrderPart> orderParts => Set<OrderPart>();
}
