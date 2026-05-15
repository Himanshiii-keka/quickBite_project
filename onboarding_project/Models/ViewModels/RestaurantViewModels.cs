namespace startup_project.Models.ViewModels
{
    // ---------- Restaurant View Model ----------

    /// <summary>
    /// ViewModel for displaying restaurant information.
    /// No navigation properties to MenuItem or Order collections.
    /// </summary>
    public class RestaurantViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
    }

    // ---------- Menu Item View Model ----------

    /// <summary>
    /// ViewModel for displaying menu item information.
    /// No navigation to Restaurant entity.
    /// </summary>
    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
