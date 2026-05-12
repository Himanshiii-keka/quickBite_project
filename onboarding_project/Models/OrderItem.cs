namespace startup_project.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // FK — indexed to load all items belonging to an order in one query
        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        // Snapshot fields — stored at order time so price changes don't affect history
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemPrice { get; set; }

        public int Quantity { get; set; }

        // UnitPrice * Quantity, computed and stored to avoid recalculation
        public decimal LineTotal { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public MenuItem MenuItem { get; set; } = null!;
    }
}
