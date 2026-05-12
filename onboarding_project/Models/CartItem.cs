namespace startup_project.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        // FK — composite unique index (CartId, MenuItemId) in DbContext so the same item
        // isn't stored as two rows; we just bump Quantity instead.
        public int CartId { get; set; }
        public int MenuItemId { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public Cart Cart { get; set; } = null!;
        public MenuItem MenuItem { get; set; } = null!;
    }
}
