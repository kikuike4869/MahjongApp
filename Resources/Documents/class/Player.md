## 🧱 `Player` クラス設計

### 📌 役割（責務）

- 手牌の管理（配牌・ツモ・捨て）
- 捨て牌の管理（河）
- 状態の保持（リーチ、親、鳴きなど）
- 和了判定や鳴き判定のための情報提供

---

### 🧾 プロパティ（メンバ変数）

```csharp
class Player
{
    public string Name { get; set; }
    public int SeatIndex { get; set; } // 0=自分, 1=下家, 2=対面, 3=上家
    public int Points { get; set; } = 25000;

    public List<Tile> Hand { get; private set; } = new();
    public List<Tile> Discards { get; private set; } = new(); // 河
    public List<Meld> OpenMelds { get; private set; } = new(); // 鳴きした面子

    public bool IsDealer { get; set; } = false;
    public bool HasDeclaredRiichi { get; private set; } = false;
    public bool IsIppatsu { get; set; } = false;
    public bool IsTenpai { get; set; } = false; // 流局時用

    public bool IsHuman { get; protected set; } = false;
}
```

---

### ⚙️ メソッド（操作）

```csharp
public void Draw(Tile tile);                  // ツモ
public virtual Tile ChooseDiscard();          // 捨てる牌を選ぶ
public void Discard(Tile tile);               // 捨てる処理
public void DeclareRiichi();                  // リーチ宣言
public void AddMeld(Meld meld);               // ポン・チー・カンなど
public bool CheckWin(Tile drawnOrClaimedTile);// 和了可能かチェック
public void SortHand();                       // 手牌をソート（見やすさ用）
```

---

### 🧠 メソッド詳細（一部例）

```csharp
public void Draw(Tile tile)
{
    Hand.Add(tile);
    SortHand();
}

public void Discard(Tile tile)
{
    if (!Hand.Contains(tile)) return;
    Hand.Remove(tile);
    Discards.Add(tile);
}

public virtual Tile ChooseDiscard()
{
    // デフォルトではランダム（AI側でオーバーライド予定）
    return Hand[new Random().Next(Hand.Count)];
}

public void DeclareRiichi()
{
    if (Hand.Count != 14 || HasDeclaredRiichi) return;
    HasDeclaredRiichi = true;
    IsIppatsu = true;
}

public bool CheckWin(Tile lastTile)
{
    // TODO: 役判定ロジックと連携して判定
    List<Tile> tempHand = new(Hand);
    tempHand.Add(lastTile);
    return YakuChecker.IsWinningHand(tempHand); // 仮の関数
}
```

---

## 🧩 派生クラスの例

### `HumanPlayer`（手動操作用）

```csharp
class HumanPlayer : Player
{
    public HumanPlayer() { IsHuman = true; }

    public override Tile ChooseDiscard()
    {
        // UI側のクリック入力などで選択された牌を返す
        return AwaitUserTileSelection();
    }
}
```

---

### `AIPlayer`（CPU）

```csharp
class AIPlayer : Player
{
    public AIPlayer() { IsHuman = false; }

    public override Tile ChooseDiscard()
    {
        // 初期段階では完全ランダム
        return base.ChooseDiscard();
        // 将来的には牌効率アルゴリズムに置き換え可
    }
}
```

---

## 🧩 補助構造：`Meld` クラス

```csharp
enum MeldType { Chi, Pon, Kan, Ankan, Shouminkan }

class Meld
{
    public MeldType Type { get; set; }
    public List<Tile> Tiles { get; set; }
    public int FromPlayerIndex { get; set; } // 鳴いた相手
}
```

---

## 🔁 他クラスとの関係

- `MahjongGameManager` が `Player` をリストで保持・制御
- `ScoreManager` や `CallManager` が `Player` の状態を参照
- `TurnManager` が `Player` の捨て・ツモ操作を呼び出す
