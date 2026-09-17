# Code Coverage Measurement Guide

This guide explains how to measure code coverage in Unity projects using various tools.

## 1. Unity Test Runner (Built-in, Basic)

### What It Does
Runs tests and provides basic test pass/fail results. **Does not measure coverage.**

### How to Use

1. **Open Test Runner**:
   - Windows → General → Test Runner

2. **Select Test Type**:
   - "EditMode" or "PlayMode" tab

3. **Run Tests**:
   - Click "Run All" button
   - Watch for green ✓ (pass) or red ✗ (fail)

4. **View Results**:
   - Test names show pass/fail status
   - Output panel shows details

### Limitations
- No coverage metrics
- No untested code identification
- Manual inspection required

---

## 2. OpenCover (Advanced, Recommended)

### What It Does
Measures precise code coverage: statements, branches, methods, cyclomatic complexity.

### Installation

**Windows**:
```powershell
# Via Chocolatey
choco install opencover

# Via NuGet (in project)
Install-Package OpenCover
```

**Alternative**: Download from [OpenCover GitHub](https://github.com/OpenCover/opencover)

### How to Use

**Option A: Command Line**

```powershell
# Run tests with coverage measurement
opencover.console.exe `
  -target:Unity.exe `
  -targetargs:"-runTests -testResults:TestResults.xml" `
  -register `
  -output:coverage.xml
```

**Option B: With Test Report**

```powershell
# Generate XML + HTML report
opencover.console.exe `
  -target:Unity.exe `
  -targetargs:"-runTests -testPlayer:./Build/MyApp.exe -testResults:TestResults.xml" `
  -register `
  -output:coverage.xml `
  -reportgenerator:"+HTMLSummary;+HTMLDetailsOpenCover"
```

### Reading the Report

**coverage.xml** structure:
```xml
<CoverageSession>
  <Summary>
    <NumSequencePoints>1234</NumSequencePoints>
    <SequenceCoverage>78.5</SequenceCoverage>  <!-- Statement coverage -->
    <BranchCoverage>72.3</BranchCoverage>      <!-- Branch coverage -->
    <NumMethods>45</NumMethods>
  </Summary>
  <Modules>
    <Module>
      <Classes>
        <Class>
          <FullyQualifiedName>YourNamespace.ClassName</FullyQualifiedName>
          <Coverage>85.0</Coverage>
          <Methods><!-- list of methods --></Methods>
        </Class>
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
```

### Interpreting Coverage Metrics

| Metric | Definition | Target |
|--------|------------|--------|
| **SequenceCoverage** | % of lines executed | 80%+ |
| **BranchCoverage** | % of conditional branches taken | 75%+ |
| **Method Coverage** | % of methods tested | 90%+ |
| **Cyclomatic Complexity** | Decision points in method | < 10 per method |

### Coverage Gaps Report

To find uncovered lines:
```powershell
# Parse XML and extract uncovered code
$xml = [xml](Get-Content coverage.xml)
$uncovered = $xml.CoverageSession.Modules.Module.Classes.Class | 
  Where-Object { [decimal]$_.Coverage -lt 100 }
$uncovered | Select-Object FullyQualifiedName, Coverage
```

---

## 3. Altcover (Alternative to OpenCover)

### What It Does
Similar to OpenCover, but lighter weight and more accurate for .NET Core projects.

### Installation

```powershell
# Via NuGet
dotnet add package altcover

# Via Chocolatey
choco install altcover
```

### How to Use

```powershell
altcover.exe `
  --inplace `
  --assemblyFilter=YourProjectName `
  -- "Unity.exe" "-runTests"

# Generate report
altcover.exe `
  --outputFile=coverage.json `
  --report=../Tools/ReportGenerator `
  -- "Unity.exe" "-runTests"
```

---

## 4. Visual Studio Code Coverage (Built-in)

### What It Does
VS Code with C# extension can show coverage gutters in editor.

### How to Use

1. **Install Extension**:
   - Ionide.Ionide-fsharp (for F#)
   - C# extension has built-in support

2. **Run Tests with Coverage**:
   ```powershell
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **View Results**:
   - Coverage report generated in `coverage.xml`
   - Open in VS Code with coverage extension

### Limitations
- Requires .NET 5+
- Limited to XPlat format
- Not ideal for large projects

---

## 5. NUnit Coverage Integration

### Direct Coverage Measurement

Some test runners include coverage. Example with Nunit Console:

```powershell
nunit3-console.exe "YourTests.dll" `
  --result=TestResult.xml;format=nunit3 `
  --labels=All
```

Then pipe to OpenCover:
```powershell
opencover.console.exe `
  -target:nunit3-console.exe `
  -targetargs:"YourTests.dll" `
  -register `
  -output:coverage.xml
```

---

## 6. JetBrains Rider (IDE Integration)

### What It Does
Integrated coverage measurement with visual gutters in editor.

### How to Use

1. **Run Tests with Coverage**:
   - Right-click test → "Run 'TestName' with Coverage"

2. **View Results**:
   - Gutters show coverage: green (covered), red (uncovered)
   - Coverage report in panel

3. **Export Report**:
   - Tools → Run Coverage → Export HTML Report

### Advantages
- No extra configuration
- Real-time feedback
- Integration with IDE refactoring

---

## 7. Automated Coverage Check (CI/CD)

### GitHub Actions Example

```yaml
name: Code Coverage

on: [push, pull_request]

jobs:
  coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '6.0'
      
      - name: Run tests with coverage
        run: |
          dotnet test --collect:"XPlat Code Coverage"
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v2
        with:
          files: ./coverage.xml
      
      - name: Check coverage threshold
        run: |
          # Parse coverage.xml and fail if < 80%
          COVERAGE=$(grep -oP 'SequenceCoverage>\K[0-9.]+' coverage.xml)
          if (( $(echo "$COVERAGE < 80" | bc -l) )); then
            echo "Coverage $COVERAGE% is below 80% threshold"
            exit 1
          fi
```

---

## 8. Reading Coverage Reports

### What Untested Code Tells You

| Finding | Interpretation | Action |
|---------|-----------------|--------|
| High branch coverage (>90%) | Logic paths well-tested | ✓ Keep testing |
| Low method coverage (<80%) | Some methods not tested | Write tests for untested methods |
| Uncovered guard clauses | Defensive code not tested | Consider if testable |
| Uncovered catch blocks | Exception paths untested | Add error scenario tests |
| Unreachable code (0% coverage) | Dead code, removed logic | Mark `[Obsolete]` or delete |

### Coverage by Component

Analyze coverage by component to prioritize:
```
Services: 92% (high) → Core logic well-tested ✓
Controllers: 65% (low) → Need more integration tests
Utils: 100% (excellent) → Complete coverage
Views: 40% (very low) → Hard to test UI, acceptable
```

---

## 9. Common Coverage Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Coverage drops after refactoring | Removed test code | Don't delete tests; refactor instead |
| Spurious branch coverage | Compiler-generated code | Use `[ExcludeFromCodeCoverage]` |
| Coverage stuck at 60% | Untestable dependencies | Inject interfaces (see Untestable Patterns) |
| Different results each run | Non-deterministic tests | Make tests deterministic (mock time) |
| Slow coverage measurement | Large test suite | Run subset: `--filter:"Category=Fast"` |

---

## 10. Coverage Best Practices

### ✅ Do

- **Test critical paths**: Focus on high-risk code (business logic, security)
- **Aim for 80%+**: Beyond 80%, diminishing returns
- **Track trends**: Monitor coverage over time
- **Automate checks**: Fail CI if coverage drops
- **Segment coverage**: Measure by component/layer
- **Document gaps**: Mark untestable code with comments

### ❌ Don't

- **Chase 100%**: Expensive, diminishing returns (defensive code, UI)
- **Test getters/setters**: Unless they contain logic
- **Mock everything**: Some integration is healthy
- **Ignore flaky tests**: Low coverage from unstable tests is misleading
- **Ignore coverage drops**: Investigate regressions immediately

---

## Quick Reference: Coverage Targets by Component

```
┌─────────────────────────────────────────────────┐
│ Coverage Target by Component                    │
├─────────────────────────────────────────────────┤
│ Business Logic (Services, Managers):  90%+      │
│ Data Models (DTOs, Entities):         85%+      │
│ API Controllers/Endpoints:            80%+      │
│ Utilities & Helpers:                  85%+      │
│ UI/Views:                             50%       │
│ Integrations (3rd party):             60%       │
│ Generated Code:                       N/A       │
└─────────────────────────────────────────────────┘

Overall Target: 80%
```
