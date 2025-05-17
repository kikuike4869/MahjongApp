using System; // Added for Random
using System.Collections.Generic; // Added for List
using System.Linq; // Added for OrderBy/ThenBy

namespace MahjongApp
{
    public class Player
    {
        public string Name { get; set; } = "unknown";
        public int SeatIndex { get; set; } // 0=自分, 1=下家, 2=対面, 3=上家
        public int Points { get; set; } = 25000;

        public List<Tile> Hand { get; protected set; } = new List<Tile>();
        public List<Tile> Discards { get; private set; } = new List<Tile>();
        public List<Meld> Melds { get; private set; } = new List<Meld>();

        public bool IsDealer { get; set; } = false;
        public bool HasDeclaredRiichi { get; private set; } = false;
        public bool IsIppatsu { get; set; } = false;
        public bool IsTenpai { get; set; } = false;
        public bool IsRiichi { get; set; } = false;
        public bool IsTsumo { get; set; } = false;

        public bool IsHuman { get; protected set; } = false;

        public virtual void Draw(Tile tile)
        {
            Hand.Add(tile);
            IsTsumo = true;
        }

        public virtual void SortHand()
        {
            if (!IsRiichi)
            {
                Hand = Hand.OrderBy(t => t.Suit).ThenBy(t => t.Number).ToList();
            }
        }

        /// <summary>
        /// Discards the specified tile from the hand. To be called by TurnManager or AI logic.
        /// </summary>
        /// <param name="tileToDiscard">The tile instance to discard.</param>
        public virtual void DiscardTile(Tile tileToDiscard)
        {
            if (Hand.Remove(tileToDiscard)) // Removes the specific instance
            {
                tileToDiscard.IsSelected = false; // Ensure selection state is reset
                Discards.Add(tileToDiscard);
                IsTsumo = false; // No longer in the state of having just drawn
                SortHand(); // Re-sort hand after discarding (optional, based on preference)
            }
            else
            {
                // This should ideally not happen if the tile comes from the player's hand correctly
                System.Diagnostics.Debug.WriteLine($"[ERROR] Attempted to discard tile not in hand: {tileToDiscard.Name()}");
                // Consider throwing an exception or handling the error state
                // throw new ArgumentException("Attempted to discard a tile that is not in the hand.");
            }


        }


        // Placeholder for AI/Human decision making - AI overrides this
        public virtual Tile? ChooseDiscardTile()
        {
            // Base implementation might return null or throw, expecting override
            throw new NotImplementedException("ChooseDiscardTile must be implemented by subclasses.");
        }

        public List<Tile> GetAllTilesInHandAndMelds()
        {
            var allTiles = new List<Tile>(this.Hand);
            foreach (var meld in this.Melds)
            {
                allTiles.AddRange(meld.Tiles);
            }
            return allTiles;
        }

        public bool IsMenzen()
        {
            // 副露（鳴き）がない状態を面前とする
            // 暗槓は面前扱い
            return !Melds.Any(m => m.IsOpen);
        }
        // public void DeclareRiichi();                  // リーチ宣言
        // public void AddMeld(Meld meld);               // ポン・チー・カンなど
        // public bool CheckWin(Tile drawnOrClaimedTile);// 和了可能かチェック
    }

    public class HumanPlayer : Player
    {
        public HumanPlayer() { IsHuman = true; }

        // Human discard choice is driven by UI interaction triggering DiscardTile directly.
        // ChooseDiscardTile might not be needed for Human if UI handles selection.
        // public override Tile ChooseDiscardTile() { ... wait for UI ... }
    }

    public class AIPlayer : Player
    {
        public AIPlayer() { IsHuman = false; }

        /// <summary>
        /// AI chooses a tile to discard (currently random).
        /// </summary>
        public override Tile? ChooseDiscardTile()
        {
            if (Hand.Count > 0)
            {
                // Use shared random instance
                Random rng = SharedRandom.Instance;
                int tileIndex = rng.Next(Hand.Count);
                return Hand[tileIndex];
            }
            return null; // Should not happen in a normal game state
        }

        // AI Discard is now typically handled by GameManager/TurnManager:
        // 1. Call AIPlayer.ChooseDiscardTile() to get the Tile.
        // 2. Call AIPlayer.DiscardTile(chosenTile) to execute the discard.
        // The old DiscardTile() override is removed as the base DiscardTile handles the action.
    }

    // Meld class definition (assuming it exists or define here)
    public class Meld
    {
        public MeldType Type { get; set; } // Pon, Chi, Kan (Ankan, MinkoKan, Kakan)
        public List<Tile> Tiles { get; set; } = new List<Tile>();
        public int FromPlayerIndex { get; set; } // 鳴いた相手 (0-3 relative to self, or absolute seat index?)
        public Tile? CalledTile { get; set; } // The tile that was called (Chi/Pon/Kan target)
        public bool IsOpen { get; set; } = false; // Open meld (Pon, Chi, Minkan) or closed (Ankan)

        // Example: Pon of Red Dragon from player to the left (index 3 relative)
        // Type = MeldType.Pon
        // Tiles = [RedDragon1, RedDragon2, RedDragon3] (where RedDragon3 was the called tile)
        // FromPlayerIndex = 3 (if relative) or specific seat index
        // CalledTile = RedDragon3
    }
}