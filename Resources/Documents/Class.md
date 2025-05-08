## 🧱 基本クラス設計（概要）

### 1. **MahjongGameManager**

- アプリ全体のゲーム進行を制御
- 局の開始・終了、親の管理、流局処理など
- 各プレイヤーのターン管理

```csharp
class MahjongGameManager
{
    List<Player> players;
    TileWall tileWall;
    TurnManager turnManager;
    ScoreManager scoreManager;

    void StartNewRound();
    void EndRound();
    void HandlePlayerTurn(Player player);
}
```

---

### 2. **Player（基底クラス）**

- プレイヤーの共通データ（手牌、点数、状態など）
- HumanPlayer / AIPlayer に派生可能

```csharp
class Player
{
    string name;
    List<Tile> hand;
    List<Tile> discards;
    int points;
    bool isDealer;
    bool hasDeclaredRiichi;
    bool isIppatsu;

    void DrawTile(Tile tile);
    void DiscardTile(Tile tile);
    void DeclareRiichi();
    bool CheckWin(Tile tile);
}
```

#### → 派生：

```csharp
class HumanPlayer : Player { /* UI操作に応じて行動 */ }
class AIPlayer : Player { /* ロジックで自動行動 */ }
```

---

### 3. **Tile（牌）**

- 牌の種類、数、赤ドラかどうかなどの情報を持つ

```csharp
enum TileSuit { Man, Pin, Sou, Honor }
enum TileHonor { East, South, West, North, White, Green, Red }

class Tile
{
    TileSuit suit;
    int number; // 1〜9 or 0（字牌の場合）
    bool isRedDora;
}
```

---

### 4. **TileWall（山）**

- 牌の山、ドラ表示牌、嶺上牌の管理

```csharp
class TileWall
{
    List<Tile> tiles;
    List<Tile> doraIndicators;

    void Shuffle();
    Tile Draw();
    Tile DrawFromDeadWall(); // カンしたとき
}
```

---

### 5. **TurnManager**

- 順番制御、ツモ・捨て進行、時間切れ処理など

```csharp
class TurnManager
{
    int currentPlayerIndex;
    Timer shortTimer;
    Timer longTimer;

    void NextTurn();
    void HandleTimeOut();
}
```

---

### 6. **CallManager（鳴き処理）**

- チー・ポン・カン・リーチの可否と実行

```csharp
class CallManager
{
    bool CanChi(Player caller, Tile discarded);
    bool CanPon(Player caller, Tile discarded);
    bool CanKan(Player caller, Tile tile);
    void ExecuteCall(Player caller, CallType type, Tile tile);
}
```

---

### 7. **ScoreManager**

- 役判定と点数計算・点棒のやり取り

```csharp
class ScoreManager
{
    bool IsWinningHand(List<Tile> hand);
    int CalculateScore(Player winner, List<Player> others, bool tsumo);
    void AdjustPoints(Player winner, List<Player> losers, int points);
}
```

---

### 8. **LogManager（任意）**

- ログの記録や表示用（デバッグ・表示用）

```csharp
class LogManager
{
    List<string> log;

    void AddLog(string message);
    void ExportLog();
}
```

---

## 🔁 クラス間の主な関係

```
MahjongGameManager
 ├─ Player（4人）
 ├─ TileWall
 ├─ TurnManager
 ├─ CallManager
 └─ ScoreManager
```
