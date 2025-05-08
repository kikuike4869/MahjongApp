## 📣 `CallManager` クラス設計

### 📌 役割（責務）

- 捨て牌に対して、各プレイヤーが鳴けるかどうかを判定する
- 鳴き（チー・ポン・カン・ロン）可能なプレイヤーの応答を収集する
- 鳴きの優先順位を判断して選択肢を絞る
- 鳴きが行われた場合の処理（ポン・チー後の順番の変更など）は `TurnManager` 側で実行

---

## 🧾 プロパティ（メンバ変数）

```csharp
class CallManager
{
    private List<Player> players;
    private int dealerSeat;

    public CallManager(List<Player> players, int dealerSeat)
    {
        this.players = players;
        this.dealerSeat = dealerSeat;
    }
}
```

---

## ⚙️ メソッド（操作）

```csharp
/// 捨て牌に対する鳴き・ロンの応答を確認（各プレイヤーに問い合わせ）
public List<CallResponse> CheckCalls(Tile discardedTile, int discarderSeat);
```

---

## 📦 `CallResponse` クラス

各プレイヤーの鳴き可否と内容を格納するデータ構造です。

```csharp
public class CallResponse
{
    public Player Player { get; set; }
    public CallType Type { get; set; }  // Ron, Pon, Chi, Kan, Pass
    public List<Tile> TilesUsed { get; set; } // 手牌で使う牌（鳴きの処理に必要）

    public CallResponse(Player player, CallType type, List<Tile> tilesUsed = null)
    {
        Player = player;
        Type = type;
        TilesUsed = tilesUsed ?? new List<Tile>();
    }
}
```

---

## 🧠 鳴き判定ロジックの概要（`CheckCalls`）

```plaintext
1. 捨て牌に対して、他3人のプレイヤーに順番に問い合わせ（上家・対面・下家）
2. 各プレイヤーの手牌から鳴き可能かをチェック：
    - チー：下家のみ、順子が作れるか
    - ポン：2枚持っていれば可
    - カン（大明槓）：3枚持っていれば可
    - ロン：和了形ができていれば可
3. ユーザー or AI に鳴くかどうかの意思決定を問い合わせ
4. ロン優先、次にポン・カン、最後にチーの順に処理（ルール準拠）
```

---

## 🔧 実装例（`CheckCalls`）

```csharp
public List<CallResponse> CheckCalls(Tile discardedTile, int discarderSeat)
{
    var responses = new List<CallResponse>();

    for (int i = 0; i < 4; i++)
    {
        if (i == discarderSeat) continue;

        Player p = players[i];
        bool isShimocha = (i == (discarderSeat + 1) % 4); // チーは下家のみ

        if (p.CanRon(discardedTile))
        {
            bool wantsRon = p.AskCallDecision(CallType.Ron, discardedTile);
            if (wantsRon)
            {
                responses.Add(new CallResponse(p, CallType.Ron));
                continue; // ロン優先のため他は調べない
            }
        }

        if (p.CanKan(discardedTile))
        {
            bool wantsKan = p.AskCallDecision(CallType.Kan, discardedTile);
            if (wantsKan)
            {
                var tilesUsed = p.GetKanTiles(discardedTile);
                responses.Add(new CallResponse(p, CallType.Kan, tilesUsed));
                continue;
            }
        }

        if (p.CanPon(discardedTile))
        {
            bool wantsPon = p.AskCallDecision(CallType.Pon, discardedTile);
            if (wantsPon)
            {
                var tilesUsed = p.GetPonTiles(discardedTile);
                responses.Add(new CallResponse(p, CallType.Pon, tilesUsed));
                continue;
            }
        }

        if (isShimocha && p.CanChi(discardedTile))
        {
            bool wantsChi = p.AskCallDecision(CallType.Chi, discardedTile);
            if (wantsChi)
            {
                var tilesUsed = p.GetChiTiles(discardedTile);
                responses.Add(new CallResponse(p, CallType.Chi, tilesUsed));
            }
        }
    }

    if (responses.Any(r => r.Type == CallType.Ron))
    {
        return responses.Where(r => r.Type == CallType.Ron).ToList();
    }

    if (responses.Any(r => r.Type == CallType.Pon || r.Type == CallType.Kan))
    {
        return responses.Where(r => r.Type == CallType.Pon || r.Type == CallType.Kan).ToList();
    }

    return responses.Where(r => r.Type == CallType.Chi).ToList();
}
```

---

## 📘 `CallType` 列挙体

```csharp
public enum CallType
{
    Pass,
    Chi,
    Pon,
    Kan,
    Ron
}
```

---

## 🤝 プレイヤー側の想定メソッド

`Player` クラスには以下のような呼び出しを想定します：

```csharp
bool CanRon(Tile tile);
bool CanChi(Tile tile);
bool CanPon(Tile tile);
bool CanKan(Tile tile);

bool AskCallDecision(CallType type, Tile tile); // 人間 or AI が鳴くか判断
List<Tile> GetChiTiles(Tile tile);              // 実際に使用する牌（UIでも必要）
List<Tile> GetPonTiles(Tile tile);
List<Tile> GetKanTiles(Tile tile);
```

---

## 🔁 他クラスとの連携

- `TurnManager` から `CheckCalls` を呼び、応答結果を受け取り優先度順に鳴き処理へ
- 鳴き成立後の処理（打牌再開・順番変更）は `TurnManager` が担当
- `Player` 側に鳴き可能性チェック・鳴き意思決定のインターフェースを実装

---

## ✅ 補足

- ロン和了は優先されるため、他の鳴き（ポン・チーなど）はすべて無視されます（1 人ロンルールを前提）
- カンの処理は暗槓・加槓などと分けて判断できるように将来的には別枠にしても OK です
- チーの選択肢（123, 234, 345 など）は UI で選択できるように、複数候補を返す仕組みも拡張可能です
