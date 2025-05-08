## 🧮 `ScoreManager` クラス設計

### 📌 役割（責務）

- 和了時の点数計算（翻・符・親/子区別）
- 点棒の受け渡し（支払い・加点）
- リーチ棒・供託棒の処理
- ノーテン罰符や流局時の点棒処理
- プレイヤーの現在得点の更新

---

## 🧾 プロパティ（メンバ変数）

```csharp
class ScoreManager
{
    private List<Player> players;         // プレイヤー4人
    private int honbaCount;               // 本場（連荘）
    private int riichiSticks;             // リーチ棒（場にある）

    public int LastPointsMoved { get; private set; } // 前回の移動点数（UI表示用など）
}
```

※ `players`, `honbaCount`, `riichiSticks` は `MahjongGameManager` から渡されるか参照

---

## ⚙️ メソッド（操作）

```csharp
public ScoreManager(List<Player> players);

public void ProcessTsumoWin(Player winner, Tile winningTile, bool isDealer, List<Yaku> yakuList);
public void ProcessRonWin(Player winner, Player discarder, Tile winningTile, bool isDealer, List<Yaku> yakuList);

public void ProcessDraw(List<Player> tenpaiPlayers);
public void AddRiichiStick(Player declarer);     // リーチ宣言時
public void TransferRiichiSticks(Player winner); // 和了時に取得

public void AddHonba();         // 本場を加算
public void ResetHonba();       // 本場をリセット
public int GetPlayerPoints(int seatIndex);
```

---

## 🔢 点数計算（概要）

点数計算は以下のように構成されます：

```plaintext
1. 符の計算（基本符30符 or 40符など）
2. 翻数の合計（役＋ドラ）
3. 子 or 親 で基本点を計算：
   子：符 × 2^(翻数+2) × 1（切り上げ100点単位）
   親：× 1.5倍（ツモ）または 6倍（ロン）

4. 本場加点：1本場ごとに+300点（ツモなら支払い元に応じて割り振り）
5. リーチ棒加点：和了者が全て回収
```

---

## 🧠 代表的メソッドの処理例

### `ProcessTsumoWin`

```csharp
public void ProcessTsumoWin(Player winner, Tile winTile, bool isDealer, List<Yaku> yakuList)
{
    int han = yakuList.Sum(y => y.Han);
    int fu = CalculateFu(winner.Hand, winTile, yakuList);

    int basePoints = CalculateBasePoints(han, fu);

    int fromNonDealer = isDealer ? basePoints * 2 : basePoints;
    int fromDealer = isDealer ? basePoints * 2 : basePoints * 2;

    foreach (var p in players)
    {
        if (p == winner) continue;
        int pay = (p.IsDealer) ? fromDealer : fromNonDealer;
        p.Points -= pay + 100 * honbaCount;
        winner.Points += pay + 100 * honbaCount;
    }

    winner.Points += riichiSticks * 1000;
    riichiSticks = 0;
    LastPointsMoved = basePoints * 6 + 300 * honbaCount;
}
```

---

### `ProcessRonWin`

```csharp
public void ProcessRonWin(Player winner, Player discarder, Tile winTile, bool isDealer, List<Yaku> yakuList)
{
    int han = yakuList.Sum(y => y.Han);
    int fu = CalculateFu(winner.Hand, winTile, yakuList);

    int basePoints = CalculateBasePoints(han, fu);
    int totalPoints = (isDealer ? basePoints * 6 : basePoints * 4);

    discarder.Points -= totalPoints + 300 * honbaCount;
    winner.Points += totalPoints + 300 * honbaCount;

    winner.Points += riichiSticks * 1000;
    riichiSticks = 0;
    LastPointsMoved = totalPoints + 300 * honbaCount;
}
```

---

### `ProcessDraw`

```csharp
public void ProcessDraw(List<Player> tenpaiPlayers)
{
    int numTenpai = tenpaiPlayers.Count;

    if (numTenpai == 0 || numTenpai == 4) return; // ノーテン罰符なし

    int pay = 3000 / (4 - numTenpai);
    int receive = 3000 / numTenpai;

    foreach (var p in players)
    {
        if (tenpaiPlayers.Contains(p)) p.Points += receive;
        else p.Points -= pay;
    }
}
```

---

### `AddRiichiStick`, `TransferRiichiSticks`

```csharp
public void AddRiichiStick(Player p)
{
    p.Points -= 1000;
    riichiSticks++;
}

public void TransferRiichiSticks(Player winner)
{
    winner.Points += riichiSticks * 1000;
    riichiSticks = 0;
}
```

---

## 🧩 補助：`Yaku` クラス例（翻数保持）

```csharp
class Yaku
{
    public string Name { get; set; }
    public int Han { get; set; }

    public Yaku(string name, int han)
    {
        Name = name;
        Han = han;
    }
}
```

---

## 🔁 他クラスとの連携

- `MahjongGameManager` → 和了者を判定後 `ScoreManager` に通知
- `Player` の点数を直接更新
- UI 層と連携して「点数移動アニメーション」や「結果表示」を反映

---

## ✅ 補足

- 実際の点数計算は `CalculateFu` や `CalculateBasePoints` など専用の細かい処理に委ねる設計が理想です（将来的に役満や複雑な符計算に対応するため）。
- 上記は標準ルールに基づいていますが、**ローカルルール対応**はオプションパラメータなどで柔軟に切り替えられる設計を目指しましょう。
