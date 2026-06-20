# Unity Unit Test Coverage Skill — Summary

This skill provides a complete workflow for writing unit tests for Unity scripts and achieving 80%+ code coverage.

## Created Files

### Main Skill
- **[SKILL.md](./SKILL.md)** — Complete 7-step workflow for writing tests and measuring coverage

### Reference Guides
- **[coverage-checklist.md](./references/coverage-checklist.md)** — Analysis template before writing tests
- **[test-matrix.md](./references/test-matrix.md)** — Common test patterns (conditionals, loops, exceptions, collections)
- **[untestable-patterns.md](./references/untestable-patterns.md)** — Solutions for hard-to-test code (Singletons, static methods, Physics, Time, etc.)
- **[nsubstitute-guide.md](./references/nsubstitute-guide.md)** — Mocking library reference and examples
- **[coverage-measurement.md](./references/coverage-measurement.md)** — Tools and techniques for measuring coverage (OpenCover, Altcover, Rider)

### Test Templates
- **[TestTemplateBasic.cs](./assets/TestTemplateBasic.cs)** — Starter template for simple classes
- **[TestTemplateAdvanced.cs](./assets/TestTemplateAdvanced.cs)** — Template for complex classes with mocking and dependencies

## Key Features

✓ **Step-by-step workflow** from analysis to coverage validation
✓ **Reusable test templates** for quick test development
✓ **Coverage patterns** for conditionals, loops, exceptions, and collections
✓ **Hard-to-test solutions** with refactoring strategies
✓ **Mocking guide** using NSubstitute
✓ **Coverage measurement tools** (OpenCover, Altcover, Rider)
✓ **Quality checklist** before declaring tests complete

## Usage

Invoke the skill in Copilot Chat:
```
/unity-unit-test-coverage
```

Provide:
1. **Script path** (e.g., `Assets/Scripts/Monsters/AI Battle Type/AiManager.cs`)
2. **Coverage target** (default: 80%)

The skill will:
- Guide analysis of the script
- Suggest test scenarios
- Provide boilerplate test code
- Help identify gaps and hard-to-test patterns

## Quick Start

1. **Use Coverage Checklist** to analyze your script
2. **Identify test scenarios** using Test Matrix
3. **Choose a template** (Basic or Advanced)
4. **Write tests** following the template structure
5. **Run tests** with Unity Test Runner
6. **Measure coverage** with OpenCover or Rider
7. **Fix gaps** using Untestable Patterns reference

## Example Scenario

**Goal**: Test `AiManager.cs` with 85% coverage

**Steps**:
1. Read [coverage-checklist.md](./references/coverage-checklist.md) → Fill out analysis
2. Find that `SimpleDecisionJob.EvaluateDecision` has branches → Use [test-matrix.md](./references/test-matrix.md)
3. Identify that `HasLineOfSight()` uses Physics.Raycast → See [untestable-patterns.md](./references/untestable-patterns.md) Solution 5
4. Copy [TestTemplateBasic.cs](./assets/TestTemplateBasic.cs) → Adapt for AiManager
5. Mock dependencies using [nsubstitute-guide.md](./references/nsubstitute-guide.md)
6. Run tests with Unity Test Runner
7. Measure coverage with [coverage-measurement.md](./references/coverage-measurement.md)
8. Fix remaining gaps and iterate

## Success Criteria

- [ ] All public methods have ≥1 test
- [ ] All branches covered (if/else, switch, loops)
- [ ] Edge cases tested (null, empty, boundaries)
- [ ] Error conditions tested (exceptions, invalid states)
- [ ] Coverage ≥ 80% overall, ≥ 90% for critical methods
- [ ] Tests pass consistently (no flaky tests)
- [ ] Code reviewed by peer
