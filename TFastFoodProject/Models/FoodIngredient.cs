using System;
using System.Collections.Generic;

namespace TFastFoodProject.Models;

public partial class FoodIngredient
{
    public int FoodId { get; set; }

    public int IngredientId { get; set; }

    public decimal? QuantityUsed { get; set; }

    public virtual Food Food { get; set; } = null!;

    public virtual Ingredient Ingredient { get; set; } = null!;
}
