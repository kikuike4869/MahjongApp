## 🔄 `TurnManager` クラス設計

### 📌 役割（責務）

- プレイヤーの手番を順に管理（0〜3 の席順）
- ツモ → 鳴き確認 → 打牌 までの流れを制御
- 鳴き（チー・ポン・カン・ロン）を受付・処理
- カン後の嶺上ツモ処理とドラ表示追加処理
- 和了・流局・局終了の判定
- 持ち時間の管理（必要であれば）

---

## 🧾 プロパティ（メンバ変数）

```csharp
class TurnManager
{
    private int currentTurnSeat;               // 現在の手番プレイヤー（0〜3）
    private int dealerSeat;                    // 親の席番号（0〜3）

    private List<Player> players;
    private TileWall tileWall;
    private CallManager callManager;
    private ScoreManager scoreManager;

    private int turnCount;                     // 累積手番数（流局判定用）
}
```

---

## ⚙️ メソッド（操作）

```csharp
public TurnManager(List<Player> players, TileWall wall, CallManager callMgr, ScoreManager scoreMgr, int dealerSeat);

public void StartNewRound();                    // 局の開始
public void StartTurn();                        // 1手番の開始（ツモ・処理）
public void ProceedAfterDiscard(Tile discardedTile); // 打牌後の鳴きチェックと次手番へ

public void ExecuteCall(Player caller, CallType callType, List<Tile> tilesUsed); // チー・ポン・カン処理
public void ExecuteWin(Player winner, Player loser, Tile winTile, bool isTsumo); // 和了処理

public bool CheckDraw();                        // 山切れ or 九種九牌等
public void EndRound();                         // 局の終了
```

---

## ⏱️ 流れ：1 ターンの進行

```plaintext
StartTurn():
  ├─ A. ツモ（tileWall.DrawTile）
  ├─ B. 手牌に追加 → 手牌制御（カン可など）
  ├─ C. 和了チェック（自摸和了）
  ├─ D. カン宣言があれば処理（→ 嶺上ツモ → ドラ追加）
  ├─ E. 打牌（プレイヤーが Tile を選択）
  └─ F. ProceedAfterDiscard() 呼び出し
```

---

### 🔁 `ProceedAfterDiscard(Tile discardedTile)`

```plaintext
  ├─ 他家が鳴けるか callManager で確認
  │    ├─ 誰かがロン → ExecuteWin()
  │    ├─ 誰かがチー・ポン・カン → ExecuteCall()
  │    └─ 誰も鳴かない → currentTurnSeat++
  └─ 次の手番 StartTurn() 再実行
```

---

## 🧠 主なメソッド詳細

### `StartNewRound()`

```csharp
public void StartNewRound()
{
    tileWall.InitializeWall(true);
    turnCount = 0;
    currentTurnSeat = dealerSeat;

    foreach (var p in players)
    {
        p.Hand = tileWall.DrawInitialHand();
    }

    StartTurn();
}
```

---

### `StartTurn()`

```csharp
public void StartTurn()
{
    if (CheckDraw())
    {
        EndRound();
        return;
    }

    Player currentPlayer = players[currentTurnSeat];
    Tile drawn = tileWall.DrawTile();
    currentPlayer.DrawTile(drawn);

    // 自摸和了 or カンチェックなど
    if (currentPlayer.CanTsumo())
    {
        ExecuteWin(currentPlayer, null, drawn, true);
        return;
    }

    if (currentPlayer.CanDeclareKan())
    {
        // プレイヤーがカン宣言するかどうかの判断が必要
    }

    // UI入力待ち → discardTile が選ばれたら ProceedAfterDiscard を呼ぶ
}
```

---

### `ProceedAfterDiscard(Tile tile)`

```csharp
public void ProceedAfterDiscard(Tile discardedTile)
{
    var responses = callManager.CheckCalls(discardedTile, currentTurnSeat);

    if (responses.Any(r => r.Type == CallType.Ron))
    {
        var winner = responses.First(r => r.Type == CallType.Ron).Player;
        ExecuteWin(winner, players[currentTurnSeat], discardedTile, false);
        return;
    }

    if (responses.Any(r => r.Type != CallType.Pass))
    {
        var chosen = responses.First(r => r.Type != CallType.Pass); // 優先順位処理
        ExecuteCall(chosen.Player, chosen.Type, chosen.TilesUsed);
        return;
    }

    currentTurnSeat = (currentTurnSeat + 1) % 4;
    turnCount++;
    StartTurn();
}
```

---

### `ExecuteCall()`

```csharp
public void ExecuteCall(Player caller, CallType callType, List<Tile> tilesUsed)
{
    caller.ExecuteCall(callType, tilesUsed);
    currentTurnSeat = caller.SeatIndex;

    if (callType == CallType.Kan)
    {
        Tile rinshan = tileWall.DrawFromDeadWall();
        caller.DrawTile(rinshan);

        tileWall.RevealNextDora();

        // 嶺上開花チェックなども必要
    }

    StartTurn();
}
```

---

### `ExecuteWin()`

```csharp
public void ExecuteWin(Player winner, Player loser, Tile winTile, bool isTsumo)
{
    var yakuList = YakuChecker.CheckYaku(winner.Hand, winTile, isTsumo, winner.IsDealer);

    if (isTsumo)
        scoreManager.ProcessTsumoWin(winner, winTile, winner.IsDealer, yakuList);
    else
        scoreManager.ProcessRonWin(winner, loser, winTile, winner.IsDealer, yakuList);

    EndRound();
}
```

---

### `CheckDraw()`

```csharp
public bool CheckDraw()
{
    return tileWall.RemainingTiles() == 0;
}
```

---

### `EndRound()`

```csharp
public void EndRound()
{
    // 点数表示、親の移動、連荘処理など
    // 次局に向けた準備は MahjongGameManager に委ねるのが理想
}
```

---

## 💡 補足

- `TurnManager` は「ゲームの中心的な流れ制御」を司る存在です。
- 鳴き（Call）や和了処理は外部の `CallManager`, `ScoreManager` に分担させて責務分離しています。
- `UI` 側との連携で「打牌の入力」「鳴き選択のポップアップ表示」などもこのクラス経由で呼び出すと綺麗になります。
- 持ち時間管理（5 秒＋ 20 秒の 2 段制）はこのクラスか `TimerManager` に分離しても構いません。
