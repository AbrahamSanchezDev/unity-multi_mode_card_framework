# Coverage Planning Checklist

Use this checklist to analyze a target script **before** writing tests. It identifies coverage gaps and helps prioritize test writing.

## 1. Script Analysis

**Script name**: ________________

**Type**: `[ ] MonoBehaviour  [ ] Service  [ ] Manager  [ ] Utility  [ ] Other: ___________`

**Total lines of code**: ______ (roughly; use LOC counter if available)

**Existing test coverage**: __________ %

---

## 2. Method Inventory

For each **public or internal** method, mark its coverage status:

| Method Name | Return Type | Parameters | Current Coverage | Priority | Notes |
|-------------|-------------|-----------|------------------|----------|-------|
| | | | ☐ None ☐ Partial ☐ Full | ☐ High ☐ Med ☐ Low | |
| | | | ☐ None ☐ Partial ☐ Full | ☐ High ☐ Med ☐ Low | |
| | | | ☐ None ☐ Partial ☐ Full | ☐ High ☐ Med ☐ Low | |

---

## 3. Decision Points

List all conditional branches that need coverage:

**Branch 1**: 
- Condition: _______________
- True path: ________________
- False path: _______________
- [ ] Both paths tested?

**Branch 2**: 
- Condition: _______________
- True path: ________________
- False path: _______________
- [ ] Both paths tested?

---

## 4. Dependencies & Hard-to-Test Patterns

### Constructor Dependencies
```
- [ ] Takes MonoBehaviour.GetComponent<>()
- [ ] Takes Scene reference
- [ ] Requires specific Game Object hierarchy
- [ ] Uses Singleton access (e.g., MyManager.Instance)
- [ ] Static method calls
```

### Runtime Dependencies
```
- [ ] Physics.Raycast / OverlapSphere
- [ ] Time.deltaTime
- [ ] Random.Range
- [ ] Instantiate / Destroy
- [ ] Event system / UnityEvents
- [ ] Coroutines
```

**Action plan for hard-to-test patterns**: (e.g., "Inject IPhysicsProvider interface")

_______________________________________________________________________________

_______________________________________________________________________________

---

## 5. Coverage Target by Category

| Category | Target | Current | Gap |
|----------|--------|---------|-----|
| **Statement coverage** (lines executed) | 80% | _____ | _____ |
| **Branch coverage** (all paths in conditionals) | 75% | _____ | _____ |
| **Method coverage** (all methods tested) | 90% | _____ | _____ |

---

## 6. Test Scenarios Needed

For each scenario, list the test(s) required:

### Scenario 1: ___________________________________
- Input: _____________________________
- Expected: __________________________
- [ ] Test written

### Scenario 2: ___________________________________
- Input: _____________________________
- Expected: __________________________
- [ ] Test written

### Scenario 3: ___________________________________
- Input: _____________________________
- Expected: __________________________
- [ ] Test written

---

## 7. Uncovered Code Analysis

**Uncovered lines/methods**:

| Code | Reason | Action |
|------|--------|--------|
| | ☐ Defensive (not critical) ☐ Unreachable ☐ Needs test | [ ] Write test [ ] Mark obsolete [ ] Keep as-is |
| | ☐ Defensive (not critical) ☐ Unreachable ☐ Needs test | [ ] Write test [ ] Mark obsolete [ ] Keep as-is |

---

## 8. Completion Sign-Off

- [ ] All public methods have ≥1 test
- [ ] All branches covered
- [ ] Hard-to-test dependencies identified with solutions
- [ ] Coverage target agreed: _______ %
- [ ] Ready to start writing tests

**Reviewer**: __________________ **Date**: __________
