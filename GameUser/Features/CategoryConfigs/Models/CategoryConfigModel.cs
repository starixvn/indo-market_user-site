using System.Collections.Generic;
using GameUser.Features.CategoryConfigs.Entities;

namespace GameUser.Features.CategoryConfigs.Models
{
    public class CategoryConfigModel
    {
        public CategoryEntity Category { get; }

        public IEnumerable<GameIconEntity> Games { get; }

        public CategoryConfigModel(CategoryEntity category, IEnumerable<GameIconEntity> games)
        {
            Category = category;
            Games = games;
        }
    }
}