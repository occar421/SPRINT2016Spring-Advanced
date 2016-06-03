## STEP1. デプロイ情報
- http://webocket-sprint2016spring.azurewebsites.net/
- Azure Web Apps, GitHub, AppVeyor

## STEP2. 必須機能の実装
- C#による実装
- RCリリースされたばかりの技術を利用
- 機能追加がしやすい設計を心掛けた
- 仕様が実装されているかの確認およびデプロイの自動化 (https://ci.appveyor.com/project/occar421/sprint2016spring-advanced)

## STEP3. 独自コマンドの実装
- time  
  現在の時間を返す
- connections or cons  
  現在接続中のクライアントのIDを返す
- help  
  コマンド一覧を返す

## 今回の開発に使用した技術
### 基本問題
- TypeScript
- Node.js
- VisualStudio Code

### 応用問題
- C#
- ASP.NET Core 1.0 RC2
- Visual Studio 2015
- Node.js (mocha)

## その他独自実装した内容の説明
- 間違ったbotコマンドに対してはUsageを出す
- SNS風デザイン + 自動スクロール付きクライアント
- JSONに"id"や"isBot"の追加、それに伴うクライアントの改良

## その他創意工夫点、アピールポイントなど
ジェバンニが一晩+αでやってくれました。  
独自機能よりも見た目を求めました。(前回同様 時間の問題により)  
自動スクロール後あえて余白を見せることで、最後のメッセージであることを感じさせます。