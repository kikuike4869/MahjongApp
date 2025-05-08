## ⏲️ `TimeManager` クラス設計

### 📌 役割（責務）

- 各プレイヤーの持ち時間（短時間・長時間）を管理
- 時間経過を監視し、タイムアウトした場合に行動（例：自摸切り）を強制
- ターン終了時や選択確定時にタイマーをリセット
- GUI 連携で残り時間の表示・視覚的カウントダウンも可能にする（拡張）

---

## 💡 想定仕様

| 種類     | 内容                                      |
| -------- | ----------------------------------------- |
| 短時間   | 1 手ごとの持ち時間（例：5 秒）            |
| 長時間   | 局開始時に補充される持ち時間（例：20 秒） |
| 時間切れ | 自動的にツモ切り or パスを実行            |
| 使用順序 | まず短時間 → 不足時に長時間を消費         |

---

## 🧾 クラス定義例

```csharp
public class TimeManager
{
    private const int ShortTimeLimit = 5;  // 秒（1手ごとの時間）
    private const int LongTimeMax = 20;    // 秒（1局ごとの時間）
    private int[] remainingLongTime;       // 各プレイヤーの長時間残量
    private CancellationTokenSource timerCts;

    public event Action<int> OnTimeout;    // タイムアウト時のコールバック（seat番号）

    public TimeManager()
    {
        remainingLongTime = new int[4]; // 4人分
        for (int i = 0; i < 4; i++) remainingLongTime[i] = LongTimeMax;
    }

    public void ResetLongTimeForRound()
    {
        for (int i = 0; i < 4; i++) remainingLongTime[i] = LongTimeMax;
    }

    public int GetRemainingLongTime(int seat) => remainingLongTime[seat];
```

---

## ⏳ ターンごとのタイマー開始

```csharp
    public async void StartTurnTimer(int seat, Action onTimeConsumed, Action onTimeout)
    {
        StopTimer(); // 既存タイマー停止
        timerCts = new CancellationTokenSource();
        CancellationToken token = timerCts.Token;

        int remainingShort = ShortTimeLimit;
        int usedLongTime = 0;

        try
        {
            while (remainingShort > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                remainingShort--;
            }

            // 短時間終了 → 長時間使用
            while (remainingShort <= 0 && remainingLongTime[seat] > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                remainingLongTime[seat]--;
                usedLongTime++;
                onTimeConsumed?.Invoke(); // 長時間消費したことを通知（UI更新など）
            }

            // 時間切れ
            if (!token.IsCancellationRequested)
                onTimeout?.Invoke();
        }
        catch (TaskCanceledException) { /* 正常終了 */ }
    }

    public void StopTimer()
    {
        timerCts?.Cancel();
    }
}
```

---

## 🔁 使用例（TurnManager から）

```csharp
timeManager.StartTurnTimer(currentSeat,
    onTimeConsumed: () => UpdateTimeDisplay(currentSeat),
    onTimeout: () => ForceDiscard(currentSeat)); // 自摸切りなどを強制
```

---

## ✅ 他クラスとの連携

| クラス        | 連携内容                                                                        |
| ------------- | ------------------------------------------------------------------------------- |
| `TurnManager` | 各プレイヤーの手番開始時に `StartTurnTimer` を呼び、終了時に `StopTimer` で停止 |
| `UI`          | 時間表示（短・長時間）のリアルタイム更新に使う（`GetRemainingLongTime`）        |
| `GameManager` | 局開始時に `ResetLongTimeForRound` を呼び出す                                   |

---

## 📘 拡張アイデア

- **秒数の可変化**：設定ファイルなどから秒数を調整可能に
- **秒単位の UI 連携**：イベントで UI 側に通知（カウントダウン）
- **プレイヤーごとの持ち時間設定**（例：AI は無制限、人間のみ制限など）
- **ポーズ・リプレイ対応**：タイマーを一時停止・再開できるように

---

## 🧪 デバッグヒント

- デバッグ時は `ShortTimeLimit = 15` などに設定して確認
- 時間切れ処理が正しく発動するか、UI 更新が止まらずに反映されるか確認

---

この設計で十分に柔軟ですが、以下の点にカスタマイズの余地があります：

- 「短時間を超えた瞬間に UI が色変化する」などの演出
- 「鳴き選択中にも時間が進むか／止めるか」
- 「リーチ後は長時間使用不可にする」などの特例ルール
