## 💠 基本 UI 構成（WinForms）

### 🧩 1. メイン画面（`MainForm`）

#### ① プレイエリア

- **自分の手牌（13 or 14 枚）**：クリックで選択、選択後に捨てる
- **他プレイヤーの河（捨て牌）**：上・左・右に配置、牌は横向き or 重ね
- **鳴き牌（副露）エリア**：チー・ポン・カンされた牌群を横並び表示
- **ドラ表示**：常に 1 枚見せる＋裏ドラの準備

#### ② 山・嶺エリア（非表示で OK）

- 実際の牌山はプレイヤーに見せる必要はない（開示はリーチ後など）

#### ③ 操作エリア

- **捨てる（打牌）**：牌を選択してクリック
- **鳴き・リーチ・ツモ等**：選択肢があればポップアップやボタンで提示
- **残り時間ゲージ**（オプション）：視覚的にカウントダウン

---

### 🧩 2. 補助画面

#### ✅ 点棒表示

- 4 人分の点数（上下左右に配置）
- リーチ棒や供託棒の表示

#### ✅ 局情報・状況

- 東 X 局 / 何本場 / 自風・場風
- 残り牌数（山の残り）

#### ✅ メッセージ表示

- 「ロン」「ツモ」「流局」「テンパイ」などを中央に演出付きで表示
- 音声 or 効果音を添えると臨場感 UP（AudioManager 使用）

---

## 🧭 操作フロー例

1. ユーザーが自摸
2. UI で手牌が 14 枚に → 捨てる牌をクリック
3. 捨て牌が河に表示される
4. 他プレイヤーの `CallManager` → UI に選択肢（ポン・チー等）を表示
5. ユーザーが鳴きを選択 → 牌を並び替えて更新
6. 点数更新・UI アニメーション → 次の手番

---

## 🧱 実装ポイント（WinForms）

| 機能              | 実装方法の例                                           |
| ----------------- | ------------------------------------------------------ |
| 牌の表示          | `PictureBox` + 画像（60x80px）を手牌や捨て牌に配置     |
| 牌のクリック      | `Click` イベントで `Player.DiscardTile()` を呼ぶ       |
| 選択 UI（鳴き等） | `Panel` に動的ボタンを表示 → 選択結果を返す            |
| 残り時間表示      | `ProgressBar` またはアニメーション付きの描画           |
| 演出メッセージ    | `Label` + アニメーション（`Timer` でフェードアウト）   |
| 描画レイヤー      | `ZIndex`（描画順）に注意、複数牌が重なる場合は工夫必要 |

---

## 🎮 UI クラスの例

```csharp
public partial class MainForm : Form
{
    private MahjongGameManager gameManager;

    public MainForm()
    {
        InitializeComponent();
        gameManager = new MahjongGameManager(this);
    }

    public void ShowDiscardChoices(List<Tile> tiles)
    {
        // 捨てる牌の選択肢をUIで提示
    }

    public void ShowCallOptions(List<CallOption> options)
    {
        // 鳴き選択をポップアップで表示
    }

    public void UpdateHandsDisplay(Player player)
    {
        // 手牌エリアの再描画
    }

    public void ShowMessage(string message)
    {
        // 「ロン！」「ツモ！」などを表示
    }
}
```

---

## 🔧 開発の流れ（UI 中心）

1. **手牌・河・副露の描画処理**の完成
2. **クリック操作 → 処理呼び出し連携**
3. **鳴き・リーチの選択ポップアップ実装**
4. **TurnManager → UI 更新の連携**
5. **ゲーム進行制御（Timer 含む）**
6. **結果演出・点数表示処理**

---

## 💡 補足

- **画像リソースの管理**：牌画像（60×80px、透過 PNG）を `ImageList` や `Dictionary<Tile, Image>` で管理すると楽です
- **UI の再描画**：重い再描画があるので、`SuspendLayout` / `ResumeLayout` で最適化
- **WinForms の限界**：アニメや画面効果は WPF や Unity の方が得意（ただし実装難度 ↑）

---
