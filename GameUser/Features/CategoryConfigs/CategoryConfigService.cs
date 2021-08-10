using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameUser.Features.FavoriteGame;

namespace GameUser.Features.CategoryConfigs
{
    public class Category
    {
        public string Name { get; set; }

        public string Key { get; set; }

        public int SortOrder { get; set; }
    }

    public class CategoryGameConfig
    {
        public int GameId { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public string IconSize { get; set; }

        public string Tag { get; set; }
    }

    public class CategoryConfig
    {
        public Category Category { get; set; }

        public IList<CategoryGameConfig> Games { get; set; }
    }

    public interface ICategoryConfigService
    {
        Task<IList<CategoryConfig>> GetConfigs(string userId);
    }

    public class CategoryConfigService : ICategoryConfigService
    {
        private readonly IFavoriteGameService favoriteGameService;

        public CategoryConfigService(IFavoriteGameService favoriteGameService)
        {
            this.favoriteGameService = favoriteGameService;
        }

        public async Task<IList<CategoryConfig>> GetConfigs(string userId)
        {
            const string favoriteKey = "favoriteGames";
            var categories = new List<CategoryConfig>
            {
                new CategoryConfig
                {
                    Category = new Category
                    {
                        Key = "allGames",
                        Name = "All Games",
                        SortOrder = 1
                    },
                    Games = new List<CategoryGameConfig>
                    {
                        new CategoryGameConfig
                        {
                            GameId = 3,
                            Name = "BauCua",
                            IconSize = "small",
                            SortOrder = 1,
                            Tag = "hot"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 5,
                            Name = "ChanLe",
                            IconSize = "small",
                            SortOrder = 2,
                            Tag = "new"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 7,
                            Name = "RongHo",
                            IconSize = "small",
                            SortOrder = 3,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 10,
                            Name = "Baccarat",
                            IconSize = "small",
                            SortOrder = 4,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 23,
                            Name = "Blackjack",
                            IconSize = "small",
                            SortOrder = 1,
                            Tag =  string.Empty
                        }
                    }
                },
                new CategoryConfig
                {
                    Category = new Category
                    {
                        Key = "cardGames",
                        Name = "Card Games",
                        SortOrder = 2
                    },
                    Games = new List<CategoryGameConfig>
                    {
                        new CategoryGameConfig
                        {
                            GameId = 3,
                            Name = "BauCua",
                            IconSize = "medium",
                            SortOrder = 1,
                            Tag = "hot"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 5,
                            Name = "ChanLe",
                            IconSize = "medium",
                            SortOrder = 2,
                            Tag = "new"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 7,
                            Name = "RongHo",
                            IconSize = "medium",
                            SortOrder = 3,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 10,
                            Name = "Baccarat",
                            IconSize = "medium",
                            SortOrder = 4,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 23,
                            Name = "Blackjack",
                            IconSize = "medium",
                            SortOrder = 1,
                            Tag =  string.Empty
                        }
                    }
                },
                new CategoryConfig
                {
                    Category = new Category
                    {
                        Key = "diceGames",
                        Name = "Dice Games",
                        SortOrder = 3
                    },
                    Games = new List<CategoryGameConfig>
                    {
                        new CategoryGameConfig
                        {
                            GameId = 3,
                            Name = "BauCua",
                            IconSize = "big",
                            SortOrder = 1,
                            Tag = "hot"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 5,
                            Name = "ChanLe",
                            IconSize = "big",
                            SortOrder = 2,
                            Tag = "new"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 7,
                            Name = "RongHo",
                            IconSize = "big",
                            SortOrder = 3,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 10,
                            Name = "Baccarat",
                            IconSize = "big",
                            SortOrder = 4,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 23,
                            Name = "Blackjack",
                            IconSize = "big",
                            SortOrder = 1,
                            Tag =  string.Empty
                        }
                    }
                },
                new CategoryConfig
                {
                    Category = new Category
                    {
                        Key = "otherGames",
                        Name = "Other",
                        SortOrder = 4
                    },
                    Games = new List<CategoryGameConfig>
                    {
                        new CategoryGameConfig
                        {
                            GameId = 3,
                            Name = "BauCua",
                            IconSize = "big",
                            SortOrder = 1,
                            Tag = "hot"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 5,
                            Name = "ChanLe",
                            IconSize = "big",
                            SortOrder = 2,
                            Tag = "new"
                        },
                        new CategoryGameConfig
                        {
                            GameId = 7,
                            Name = "RongHo",
                            IconSize = "big",
                            SortOrder = 3,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 10,
                            Name = "Baccarat",
                            IconSize = "big",
                            SortOrder = 4,
                            Tag = string.Empty
                        },
                        new CategoryGameConfig
                        {
                            GameId = 23,
                            Name = "Blackjack",
                            IconSize = "big",
                            SortOrder = 1,
                            Tag =  string.Empty
                        }
                    }
                }
            };
            var allGames = new List<CategoryGameConfig>();

            foreach (var category in categories)
            {
                allGames.AddRange(category.Games);
            }

            var favoriteGames = await GetFavoriteGameConfigs(userId, allGames);
            var favoriteCateogry = new CategoryConfig
            {
                Category = new Category
                {
                    Key = favoriteKey,
                    Name = string.Empty,
                    SortOrder = 0
                },
                Games = favoriteGames
            };

            categories.Add(favoriteCateogry);

            return categories;
        }

        private async Task<IList<CategoryGameConfig>> GetFavoriteGameConfigs(string userId, IList<CategoryGameConfig> allGames)
        {
            var favorite = await favoriteGameService.GetFavoriteGames(userId);
            var games = new List<CategoryGameConfig>();

            foreach (var gameId in favorite.Games)
            {
                var game = allGames.FirstOrDefault(game => game.GameId == gameId);

                if (game != null)
                {
                    const string defaultIconSize = "medium";
                    var favoriteGame = new CategoryGameConfig
                    {
                        GameId = gameId,
                        IconSize = defaultIconSize,
                        Name = game.Name,
                        SortOrder = games.Count(),
                        Tag = string.Empty
                    };

                    games.Add(favoriteGame);
                }
            }

            return games;
        }
    }
}