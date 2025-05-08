## 🧱 `TileWall` クラス設計

### 📌 役割（責務）

- ゲーム開始時に 136 枚の牌を生成・シャッフル
- ドラ表示牌の抽出・保持
- プレイヤーへの配牌処理
- 通常のツモ・嶺上ツモ処理
- 山の残り枚数管理（流局判定用）

---

### 🧾 プロパティ（メンバ変数）

```csharp
class TileWall
{
    private List<Tile> allTiles;            // シャッフル済み136枚の山
    private Queue<Tile> liveWall;           // 通常のツモ山（最初の122枚）
    private Queue<Tile> deadWall;           // 嶺上牌＋ドラ表示牌（14枚）

    public List<Tile> DoraIndicators { get; private set; } = new(); // 表ドラ
    public List<Tile> UraDoraIndicators { get; private set; } = new(); // 裏ドラ（リーチ時用）
}
```

---

### ⚙️ メソッド（操作）

```csharp
public void InitializeWall(bool useRedDora);   // 山の生成＋シャッフル
public List<Tile> DrawInitialHand();           // 配牌用（13枚）
public Tile DrawTile();                        // 通常のツモ
public Tile DrawFromDeadWall();                // 嶺上牌からツモ
public Tile RevealNextDora();                  // カンでドラ追加
public Tile RevealUraDora();                   // 裏ドラ表示（リーチのみ）
public int RemainingTiles();                   // ツモ山残り枚数（流局判定用）
```

---

## 🔧 メソッド詳細（例）

### `InitializeWall()`

```csharp
public void InitializeWall(bool useRedDora)
{
    allTiles = new List<Tile>();

    // 萬子, 筒子, 索子 各4枚×9種
    foreach (TileSuit suit in new[] { TileSuit.Man, TileSuit.Pin, TileSuit.Sou })
    {
        for (int num = 1; num <= 9; num++)
        {
            for (int i = 0; i < 4; i++)
            {
                bool isRed = (useRedDora && num == 5 && i == 0);
                allTiles.Add(new Tile(suit, num, isRed));
            }
        }
    }

    // 字牌（東南西北白発中） 各4枚
    foreach (TileHonor honor in Enum.GetValues(typeof(TileHonor)))
    {
        for (int i = 0; i < 4; i++)
        {
            allTiles.Add(new Tile(TileSuit.Honor, 0, false, honor));
        }
    }

    // シャッフル
    allTiles = allTiles.OrderBy(x => Guid.NewGuid()).ToList();

    // 山を分割
    deadWall = new Queue<Tile>(allTiles.TakeLast(14));
    liveWall = new Queue<Tile>(allTiles.Take(allTiles.Count - 14));

    // 表ドラ1枚表示（最初の5枚目）
    DoraIndicators.Clear();
    DoraIndicators.Add(deadWall.ElementAt(4));
}
```

---

### `DrawTile()`

```csharp
public Tile DrawTile()
{
    if (liveWall.Count == 0) return null; // 山切れ＝流局
    return liveWall.Dequeue();
}
```

---

### `DrawFromDeadWall()`

```csharp
public Tile DrawFromDeadWall()
{
    if (deadWall.Count == 0) return null; // 嶺上切れ
    return deadWall.Dequeue(); // カン時のツモ
}
```

---

### `RevealNextDora()`

```csharp
public Tile RevealNextDora()
{
    if (DoraIndicators.Count >= 5) return null; // 最大5枚
    Tile nextDora = deadWall.ElementAt(4 + DoraIndicators.Count);
    DoraIndicators.Add(nextDora);
    return nextDora;
}
```

---

### `RevealUraDora()`

```csharp
public Tile RevealUraDora()
{
    if (UraDoraIndicators.Count >= DoraIndicators.Count) return null;
    Tile ura = deadWall.ElementAt(UraDoraIndicators.Count);
    UraDoraIndicators.Add(ura);
    return ura;
}
```

---

### `RemainingTiles()`

```csharp
public int RemainingTiles() => liveWall.Count;
```

---

## 📌 注意点・補足

- **赤ドラ**は任意で生成（萬子・筒子・索子の 5 に各 1 枚）→ 最初の 1 枚だけ赤に
- **ドラ表示牌**は `deadWall[4], 6, 8, 10, 12`（カン回数によって追加）
- **嶺上牌**も `deadWall` からカン時に 1 枚ずつ取り出す
- 山が尽きたら即流局（ツモができなくなる）

---

このクラスは麻雀において重要な「山とドラ」の管理を一手に引き受けます。
UI と連携して「山牌の残数表示」「ドラ表示牌の可視化」もできます。
