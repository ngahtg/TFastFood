using System;
using System.Collections.Generic;

namespace TFastFoodProject.Models;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public decimal? StockQuantity { get; set; }

    public virtual ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
}
