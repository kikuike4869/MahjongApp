## 🧩 `MahjongGameManager` クラス設計

### 📌 役割（責務）

このクラスは、以下の麻雀ゲームの基本サイクルを統括します：

- ゲーム開始〜終了までの制御
- 各局（東 1 局など）の進行管理
- 親・本場・供託の制御
- プレイヤーの順番にターンを渡す
- 局の終了条件判定（ツモ・ロン・流局）
- 次局またはゲーム終了処理の判断

---

### 🧾 プロパティ（メンバ変数）

```csharp
class MahjongGameManager
{
    List<Player> players;             // プレイヤー（自分＋CPU3人）
    TileWall tileWall;                // 牌山
    TurnManager turnManager;          // 順番・時間制御
    CallManager callManager;          // 鳴き・リーチ等の処理
    ScoreManager scoreManager;        // 点数処理
    LogManager logManager;            // 履歴記録（任意）

    int roundNumber;                  // 0: 東1局, 1: 東2局 ...
    int honbaCount;                   // 本場（連続流局/連荘）
    int riichiSticks;                 // 卓上のリーチ棒
    int dealerIndex;                  // 親プレイヤーのインデックス（0〜3）
}
```

---

### ⚙️ メソッド（関数）

```csharp
void StartGame();
void StartRound();
void ProcessTurn();
bool CheckWinOrDraw();
void EndRound();
void AdvanceRound();
void EndGame();
```

---

### 🔁 ゲーム進行フロー（簡略）

```plaintext
StartGame()
 └─ for each kyoku:
       └─ StartRound()
             └─ while (!CheckWinOrDraw()):
                   └─ ProcessTurn() ← ツモ → 鳴き処理 → 捨て → 時間管理
             └─ EndRound()
       └─ AdvanceRound()
 └─ EndGame()
```

---

### 🧠 各メソッド詳細（中身の概要）

#### `StartGame()`

- プレイヤー 4 人作成
- 親をランダムまたは 0 番に設定
- 得点初期化、牌画像読み込みなど

#### `StartRound()`

- 山牌を生成・シャッフル
- 各プレイヤーに 13 枚配牌（親は 14 枚）
- ドラ表示牌設定
- ユーザーに画面を初期化表示

#### `ProcessTurn()`

- 現在のプレイヤーがツモ
- 和了判定（ツモアガリ）
- 鳴きの可能性チェック
- 手牌から捨て牌を選ぶ（時間切れ → 自摸切り）
- 他家がロンできるか判定
- 次のプレイヤーへ進む

#### `CheckWinOrDraw()`

- 誰かがツモ or ロン していたら true
- 流局条件（山がなくなる・四風連打など）を確認

#### `EndRound()`

- 点数計算・点棒移動
- 親の継続判断（アガリ連荘 or ノーテン終了）
- 本場/供託の処理
- ログ追加など

#### `AdvanceRound()`

- 局数を進める（親が続投 or 順番が進む）
- 東 → 南の判断（南場までやるなら）

#### `EndGame()`

- 点数表示
- 最終結果表示（トップ・ラスなど）

---

## 📌 補足

- 状態管理が複雑になるため、**状態遷移を Enum 化**すると管理が楽になります（例：`GameState.Dealing, Waiting, TurnInProgress, RoundEnd` など）。
- ゲーム進行の UI 更新やアニメーション処理は**イベントベース**で他クラスへ通知して分離します（例：`OnRoundStart`, `OnTurnEnd`, `OnGameEnd`）。

---

必要に応じて、このクラスはシングルトンにしてもいいですが、C#では GameManager 風に Form と組んで管理する形がよく見られます。
