# Harness Demo: Run Sheet

Sample solution for the article **"Your AI Agent Isn't the Problem. Your .NET Repo Has No Harness."**

Every step below was replayed end to end on .NET SDK 10.0.112. The expected output is real, not mocked.
Run time for a reader: about 8 minutes at a relaxed pace.

Commands use `cp`, `rm`, `mkdir` and `git`, which work the same in Git Bash, macOS/Linux shells and PowerShell.

---

## What is in the project

```text
harness-demo/
├─ AGENTS.md                 Step 1: the rules file
├─ Shop.sln
├─ src/
│  ├─ Shop.Domain/           Order, Customer (Customer is nullable on purpose)
│  ├─ Shop.Application/      IOrderRepository, GetOrderHandler, AssemblyMarker
│  ├─ Shop.Infrastructure/   EF Core (in-memory), OrderRepository
│  └─ Shop.Api/              minimal API composition root
└─ demo/                     files you copy in, one folder per article step
   ├─ step2-violation/       CancelOrderHandler.cs  (uses EF Core directly)
   ├─ step3-test/            LayerRules.cs          (NetArchTest rule)
   ├─ step3-fix/             CancelOrderHandler.cs  (uses IOrderRepository)
   ├─ step4-analyzers/       Directory.Build.props, NullBug.cs
   └─ step5-hook/            pre-commit
```

The baseline is clean: it builds with 0 warnings and 0 errors. The architecture test project, the analyzer config and the hook do not exist yet. You add them live, exactly as the article does.

---

## One-time setup (about 2 minutes)

You need the .NET 10 SDK, Git, a terminal and an editor. Check with `dotnet --version` (should start with 10).
The projects target `net10.0` and use EF Core 10.0.12.

```bash
cd harness-demo
git init
git add -A
git commit -m "baseline"
git tag baseline
dotnet build
```

Expect:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The first restore needs internet access for NuGet.

---

## Step 1: A Rules File Is a Wish

**Do:** open the repo in your editor with `AGENTS.md` and the four projects visible.

**Say:** the rule is written down. Nothing checks it.

> **[SCREENSHOT 1]** Solution Explorer showing the four projects and `AGENTS.md` at the repo root.

---

## Step 2: The Agent Breaks It and the Build Stays Green

**Option A: with your agent.** Prompt it: `Add an endpoint to cancel an order.`
Agents do not always take this shortcut. If yours uses the repository correctly, use Option B so the take is repeatable.

**Option B: scripted.**

```bash
dotnet add src/Shop.Application package Microsoft.EntityFrameworkCore --version 10.0.12
cp demo/step2-violation/CancelOrderHandler.cs src/Shop.Application/Orders/
dotnet build
```

Expect:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

> **[SCREENSHOT 2]** terminal showing `Build succeeded. 0 Warning(s) 0 Error(s)` right after the change.

**Say:** green build, rule broken. The compiler never asked whether it belongs.

---

## Step 3: Turn the Rule Into a Test That Fails Loudly

```bash
dotnet new xunit -n Shop.ArchitectureTests -o tests/Shop.ArchitectureTests
dotnet sln add tests/Shop.ArchitectureTests
dotnet add tests/Shop.ArchitectureTests package NetArchTest.Rules
dotnet add tests/Shop.ArchitectureTests reference src/Shop.Application
rm tests/Shop.ArchitectureTests/UnitTest1.cs
cp demo/step3-test/LayerRules.cs tests/Shop.ArchitectureTests/
dotnet test
```

Expect (key lines):

```text
[xUnit.net ...]     Shop.ArchitectureTests.LayerRules.ApplicationMustNotUseEfCore [FAIL]
  Error Message:
   Application must not use EF Core. Depend on IOrderRepository (Application.Abstractions). Offenders: Shop.Application.Orders.CancelOrderHandler
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1
```

> **[SCREENSHOT 3]** terminal showing the failed test with `CancelOrderHandler` named as the offender and the `IOrderRepository` instruction.

**Say:** the failure message is the agent's next prompt.

Note: do not use `-v q` on `dotnet test` here. Quiet mode hides the error message, and the message is the whole point.

---

## Step 4: Make the Compiler Say No

```bash
cp demo/step4-analyzers/Directory.Build.props .
cp demo/step4-analyzers/NullBug.cs src/Shop.Application/Orders/
dotnet build
```

Expect:

```text
src/Shop.Application/Orders/NullBug.cs(10,20): error CS8602: Dereference of a possibly null reference.
    1 Error(s)
```

> **[SCREENSHOT 4]** build output showing `error CS8602: Dereference of a possibly null reference.`

Now remove the bug so Step 5 shows only the architecture failure:

```bash
rm src/Shop.Application/Orders/NullBug.cs
dotnet build
```

Expect `0 Warning(s)` and `0 Error(s)`. The test from Step 3 is still failing, which is what Step 5 needs.

---

## Step 5: Close the Loop Before the Commit

```bash
mkdir .githooks
cp demo/step5-hook/pre-commit .githooks/pre-commit
chmod +x .githooks/pre-commit
git config core.hooksPath .githooks
git add -A
git commit -m "cancel order"
```

`chmod` is only needed on macOS, Linux and Git Bash. Skip it in plain PowerShell.

Expect: the build succeeds, then the same failed test and message as Step 3, and the commit is **not** created. Confirm with `git log --oneline` (only `baseline` appears).

> **[SCREENSHOT 5]** terminal showing the pre-commit hook output ending in the failed architecture test and its message; no commit created.

**The fix (the payoff beat).** Apply what the message asked for, then commit again:

```bash
cp demo/step3-fix/CancelOrderHandler.cs src/Shop.Application/Orders/CancelOrderHandler.cs
dotnet remove src/Shop.Application package Microsoft.EntityFrameworkCore
git add -A
git commit -m "cancel order"
```

Expect: tests pass (`Passed! - Failed: 0, Passed: 1`) and the commit goes through.

**Optional bonus take (the loop).** Before the fix, give your agent the same prompt again plus the failing test output, and let it correct itself. That shows the loop in the diagram working with a real agent.

---

## Step 6 (video only): the catch beat, the hook can be skipped

Run this right after the blocked commit in Step 5, **before** applying the fix, while the violation is still in the repo.
The article does not include this step; the YouTube script uses it for the "wait, there's a catch" beat.

```bash
git commit --no-verify -m "cancel order"
git log --oneline
dotnet test
```

Expect: the commit is created, `git log --oneline` shows two commits (`cancel order` and `baseline`), and `dotnet test` still fails with the same architecture message.

> **[SCREENSHOT 6]** terminal showing the `--no-verify` commit succeeding, `git log --oneline` with two commits, and the architecture test still failing.

If you already applied the fix, run the reset below and replay Steps 2 to 5 first.

---

## Reset between takes

```bash
git reset --hard baseline
git clean -fdx
git config --unset core.hooksPath
dotnet build
```

`git clean -fdx` also removes `bin/` and `obj/`, so the next build takes a few seconds longer.

---

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| Hook does not run | Run `git config core.hooksPath`. It must print `.githooks`. Re-run the `git config` line from Step 5. |
| `\r: command not found` on Windows | The hook has CRLF line endings. `.gitattributes` in this repo forces LF; re-copy the hook after the baseline commit. |
| `CA1050` / `CA1707` errors in Step 4 | The test file must live in a namespace and use PascalCase method names. `demo/step3-test/LayerRules.cs` already does. |
| Restore fails | NuGet needs network access on the first run. |
| `dotnet --version` is not 10.x | Install the .NET 10 SDK. If you must stay on an older SDK, change `net10.0` in each `.csproj` and the EF Core package versions to match. |

---

## Why the demo files differ slightly from the article

The article's test snippet and hook were updated to match this project. Two things only surface when you run them with the Step 4 analyzers on: the test class needs a namespace and a PascalCase method name (otherwise `CA1050`/`CA1707` break the build), and the hook must not use `-v q` on `dotnet test` (otherwise the failure message the agent needs is hidden).
