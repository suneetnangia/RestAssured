namespace RestAssured.Models
{
    public class Order
    {
        public Order(string id)
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id : throw new System.ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
        }

        public string Id { get; }
    }
}