---
name: unity-unit-test-coverage
description: 'Write unit tests for Unity scripts using NUnit and Unity Test Framework to achieve full or high code coverage. Use when: testing MonoBehaviours, Services, Managers, and utility classes; aiming for >80% code coverage; integrating new features; refactoring critical systems; ensuring regression prevention.'
argument-hint: 'Provide the script path or class name to test, and the desired coverage target (default: 80%)'
user-invocable: true
---

# Unity Unit Test Coverage Skill

## Purpose

This skill guides you through creating comprehensive unit tests for Unity scripts to achieve full or high code coverage (80%+). It covers strategy, setup, test patterns, and coverage validation.

## When to Use

- **Writing new tests** for untested MonoBehaviours, Services, or Managers
- **Improving coverage** on existing scripts—targeting specific methods or branches
- **Feature integration** to ensure new code paths are tested
- **Refactoring critical systems** to prevent regressions
- **Preparing code for production** and quality gates
- **Debugging hard-to-test code** (dependencies, static refs, singletons)

## Prerequisites

- **NUnit** or **Unity Test Framework** installed (latest)
- Test project structure: `Assets/Tests/` or `Assets/Tests/Editor/` folder
- **OpenCover** or **Altcover** for coverage measurement (optional but recommended)
- Target script fully readable and debuggable

## Step-by-Step Workflow

### 1. Analyze the Target Script

Before writing tests, understand the code structure:

1. **Identify test-critical methods**: Public methods, exposed properties, event handlers
2. **Map decision branches**: Conditions, loops, switch statements that need coverage
3. **Note dependencies**: Constructor parameters, MonoBehaviour lifecycle hooks, static references
4. **Flag hard-to-test patterns**: Singletons, Scene references, Physics casts, Time.deltaTime

### 2. Set Up Test Infrastructure

Choose the appropriate test type:

| Test Type | Location | Scope | When to Use |
|-----------|----------|-------|------------|
| Play Mode | `Assets/Tests/` | Full Unity context | MonoBehaviours, Scene interactions, Physics |
| Edit Mode | `Assets/Tests/Editor/` | No runtime, instant | Services, Utils, logic-heavy classes |
| Hybrid | Both folders | Mixed | Complex systems |

### 3. Design Test Scenarios

For each method, identify:
- **Happy path** (normal success case)
- **Edge cases** (empty input, null, boundaries)
- **Error conditions** (exceptions, invalid state)
- **State transitions** (before/after effects)

### 4. Write Tests Following Coverage Rules

**Coverage Rule**: Every code path should be exercised by at least one test.

Key patterns:
- **Branch coverage**: Test `if/else`, `switch`, ternary operators—hit both branches
- **Loop coverage**: Test empty, single-item, and multi-item iterations
- **Exception coverage**: Verify try/catch/finally paths execute
- **Null handling**: Test null inputs if the method accepts nullable types
- **State coverage**: Test state-dependent behavior (enabled/disabled, alive/dead)

### 5. Run Tests and Verify Coverage

**With Unity Test Runner**:
1. Window → General → Test Runner
2. Run all tests in Edit or Play mode
3. Verify all tests pass (green ✓)

## Common Coverage Patterns

### Conditional Coverage
```csharp
if (value > 0)
    DoSomething();
else
    DoOtherThing();

// Tests needed:
// [Test] public void WhenValueGreaterThanZero_DoSomething()
// [Test] public void WhenValueLessThanZero_DoOtherThing()
```

### Loop Coverage
```csharp
foreach (var item in items) { ... }

// Tests needed:
// [Test] public void WithEmptyList_DoesNothing()
// [Test] public void WithMultipleItems_ProcessesAll()
```

### Exception Coverage
```csharp
try { ... }
catch (InvalidOperationException ex) { ... }
finally { ... }

// Tests needed:
// [Test] public void WhenExceptionThrown_CatchBlockExecutes()
// [Test] public void FinallyBlockAlwaysExecutes()
```

See [Advanced Test Patterns](./assets/TestTemplateAdvanced.cs) for complete code examples.

## Handling Hard-to-Test Code

| Pattern | Problem | Solution |
|---------|---------|----------|
| **Singleton** | Can't inject, can't reset | Extract interface, use mock in test |
| **Static method** | No dependency injection | Use NSubstitute library for mocking |
| **Scene reference** | Requires scene loaded | Use Play Mode test + test scene |
| **Physics cast** | Non-deterministic | Mock Physics in Edit Mode; use deterministic setup |
| **Coroutine** | Time-dependent | Use `WaitForSeconds` → Play Mode + frame advance |
| **Time.deltaTime** | Global state | Inject time provider interface, mock it |

## Quality Checklist

Before declaring tests complete:

- [ ] All public methods have ≥1 test
- [ ] Each conditional branch has ≥1 test  
- [ ] Null/empty inputs tested where applicable
- [ ] Error conditions tested (exceptions, invalid states)
- [ ] State transitions verified (before/after)
- [ ] Tests pass consistently (no flaky tests)
- [ ] Coverage ≥ 80% overall, ≥ 90% for methods
- [ ] Tests documented with clear intent comments
- [ ] Redundant tests removed or consolidated
- [ ] Code review completed
