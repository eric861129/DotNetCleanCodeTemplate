---
name: clean-code-architecture-guardian
description: Use this skill whenever reviewing, modifying, or planning changes in this CleanCodeTemplate repository. It enforces the repository's Traditional Chinese Clean Code standards, Clean Architecture dependency direction, naming rules, and layer responsibilities. Trigger for code review, refactoring, architecture decisions, "這樣放對嗎", "幫我檢查架構", or any change that may cross Domain/Application/Infrastructure/WebApi/Worker boundaries.
---

# Clean Code Architecture Guardian

使用此 skill 來保護本專案的 Clean Code 與 Clean Architecture 邊界。回答與實作時以繁體中文為主，保留必要英文技術名詞、型別名稱、指令與路徑。

## 必讀文件

開始前先閱讀：

1. `docs/clean-code-standards.md`
2. `docs/architecture.md`
3. `docs/development-workflow.md`

只讀需要的段落即可，但不要忽略這三份文件的規範。

## 核心判斷

先問自己：

- 這段程式的責任屬於哪一層？
- 它是否依賴了不該依賴的外層細節？
- 命名是否清楚表達用途？
- 方法是否只做一件明確的事？
- 業務規則是否放在 Domain 或 Application？
- 是否容易寫 unit test？
- 是否出現 `Helper`、`Utility`、`Manager`、`Misc`、`Temp` 這類模糊命名？

## 分層規則

- Domain 不可 reference Application、Infrastructure、WebApi、Worker。
- Application 可以 reference Domain。
- Infrastructure 可以 reference Application 與 Domain。
- WebApi 可以 reference Application 與 Infrastructure。
- Worker 可以 reference Application 與 Infrastructure。
- Tests 可以 reference 被驗證的層級。

## 審查輸出格式

若使用者要求 review，請用這個格式：

```markdown
## 發現
- [Severity] `path/to/file.cs:line` 問題說明與影響

## 建議
- 建議修改方向

## 驗證
- 已執行或建議執行的驗證命令
```

若沒有發現問題，直接說明「目前沒有發現違反 Clean Code / Clean Architecture 的問題」，並補充仍建議執行哪些測試。

## 修改守則

- 不要為了抽象而抽象；只有當抽象能降低重複、隔離外部細節或清楚表達 use case 時才新增。
- 不要把業務規則塞進 endpoint、EF mapping、Worker loop。
- 不要讓 Infrastructure 型別洩漏到 Domain 或 Application 的 public contract。
- 不要用註解補救不清楚的命名；優先改善名稱與結構。
- 遵守 repository 指示：禁止批量刪除檔案或目錄。
