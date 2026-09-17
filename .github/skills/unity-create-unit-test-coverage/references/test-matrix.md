# Test Scenario Matrix — Common Coverage Patterns

This reference provides reusable test patterns for common code structures.

## 1. Conditional Branch Coverage

### Pattern: Simple if/else

**Code to test**:
```csharp
public void Activate(bool condition)
{
    if (condition)
        EnableFeature();
    else
        DisableFeature();
}
```

**Test matrix**:
```
┌─────────────────────┬──────────┬────────────────────┐
│ Scenario            │ Input    │ Expected           │
├─────────────────────┼──────────┼────────────────────┤
│ Condition is true   │ true     │ EnableFeature()    │
│ Condition is false  │ false    │ DisableFeature()   │
└─────────────────────┴──────────┴────────────────────┘
```

**Test code**:
```csharp
[Test] public void WhenConditionTrue_EnableFeature() => Assert.That(Activate(true), Is.True);
[Test] public void WhenConditionFalse_DisableFeature() => Assert.That(Activate(false), Is.False);
```

---

### Pattern: Nested if/else (Multiple decision points)

**Code to test**:
```csharp
public string Evaluate(int health, bool isAlive)
{
    if (isAlive)
    {
        if (health > 50)
            return "Healthy";
        else
            return "Injured";
    }
    else
        return "Dead";
}
```

**Test matrix**:
```
┌──────────────┬────────────┬────────────┬──────────────┐
│ isAlive      │ health     │ Result     │ Test Name    │
├──────────────┼────────────┼────────────┼──────────────┤
│ true         │ > 50       │ Healthy    │ AliveAndHealthy |
│ true         │ ≤ 50       │ Injured    │ AliveAndInjured |
│ false        │ (any)      │ Dead       │ Dead         |
└──────────────┴────────────┴────────────┴──────────────┘
```

**Note**: When `isAlive = false`, the `health` check is skipped. Test coverage requires **3 tests** to hit all branches.

---

## 2. Loop Coverage

### Pattern: foreach Loop

**Code to test**:
```csharp
public int SumAll(List<int> numbers)
{
    int sum = 0;
    foreach (var num in numbers)
        sum += num;
    return sum;
}
```

**Test matrix**:
```
┌─────────────────────────────┬──────────┬─────────┐
│ Scenario                    │ Input    │ Result  │
├─────────────────────────────┼──────────┼─────────┤
│ Empty list                  │ []       │ 0       │
│ Single item                 │ [5]      │ 5       │
│ Multiple items              │ [1,2,3]  │ 6       │
└─────────────────────────────┴──────────┴─────────┘
```

**Test code**:
```csharp
[Test] public void WithEmptyList_ReturnsZero() 
    => Assert.That(SumAll(new List<int>()), Is.EqualTo(0));

[Test] public void WithSingleItem_ReturnsThatItem() 
    => Assert.That(SumAll(new List<int> { 5 }), Is.EqualTo(5));

[Test] public void WithMultipleItems_ReturnSum() 
    => Assert.That(SumAll(new List<int> { 1, 2, 3 }), Is.EqualTo(6));
```

---

### Pattern: while Loop with Break

**Code to test**:
```csharp
public bool FindValue(List<int> items, int target)
{
    foreach (var item in items)
    {
        if (item == target)
            return true;
    }
    return false;
}
```

**Test matrix**:
```
┌──────────────────────────┬─────────────────────┬─────────┐
│ Scenario                 │ Input               │ Result  │
├──────────────────────────┼─────────────────────┼─────────┤
│ Item not in list         │ [1,2,3], target=99  │ false   │
│ Item at start            │ [5,2,3], target=5   │ true    │
│ Item in middle           │ [1,5,3], target=5   │ true    │
│ Item at end              │ [1,2,5], target=5   │ true    │
└──────────────────────────┴─────────────────────┴─────────┘
```

---

## 3. Exception Handling Coverage

### Pattern: try/catch/finally

**Code to test**:
```csharp
public bool ProcessFile(string path)
{
    try
    {
        var text = File.ReadAllText(path);
        ParseData(text);
        return true;
    }
    catch (FileNotFoundException ex)
    {
        Debug.LogError($"File not found: {path}");
        return false;
    }
    finally
    {
        Debug.Log("Processing complete");
    }
}
```

**Test matrix**:
```
┌──────────────────────────┬──────────────────────┬─────────────────────┐
│ Scenario                 │ Setup                │ Expected            │
├──────────────────────────┼──────────────────────┼─────────────────────┤
│ File exists              │ File at path exists  │ true (try executes) │
│ File not found           │ Path invalid         │ false (catch)       │
│ Finally always runs      │ Any condition        │ "Processing log"    │
└──────────────────────────┴──────────────────────┴─────────────────────┘
```

**Test code**:
```csharp
[Test] public void WhenFileExists_ReturnsTrueAndRunsFinally()
{
    // Setup: Create temp file
    string tempFile = Path.GetTempFileName();
    File.WriteAllText(tempFile, "test");
    
    bool result = ProcessFile(tempFile);
    
    Assert.That(result, Is.True);
    // Verify finally block ran (check side effects, e.g., log)
    
    // Cleanup
    File.Delete(tempFile);
}

[Test] public void WhenFileMissing_ReturnsFalse()
{
    bool result = ProcessFile("/nonexistent/path");
    Assert.That(result, Is.False);
}
```

---

## 4. Null/Default Input Coverage

### Pattern: Guard Clauses

**Code to test**:
```csharp
public void ProcessItem(Item item)
{
    if (item == null)
        return; // Guard clause
    
    item.UpdateValue(100);
}
```

**Test matrix**:
```
┌──────────────────────────┬───────┬──────────────────────┐
│ Scenario                 │ Input │ Expected             │
├──────────────────────────┼───────┼──────────────────────┤
│ Item is null             │ null  │ Return early         │
│ Item is valid            │ Item  │ UpdateValue called   │
└──────────────────────────┴───────┴──────────────────────┘
```

**Test code**:
```csharp
[Test] public void WhenItemNull_DoesNotThrow()
{
    Assert.DoesNotThrow(() => ProcessItem(null));
}

[Test] public void WhenItemValid_UpdatesCalled()
{
    var item = new Item();
    ProcessItem(item);
    Assert.That(item.Value, Is.EqualTo(100));
}
```

---

## 5. State-Dependent Behavior

### Pattern: State Machine Methods

**Code to test**:
```csharp
public class Monster
{
    private bool isAlive = true;
    
    public void TakeDamage(int amount)
    {
        if (!isAlive)
            return; // Already dead
        
        health -= amount;
        if (health <= 0)
            isAlive = false;
    }
}
```

**Test matrix**:
```
┌────────────────────────────┬──────────────────────┬──────────────┐
│ Scenario                   │ Setup                │ Result       │
├────────────────────────────┼──────────────────────┼──────────────┤
│ Take damage while alive    │ isAlive=T, hp=100    │ hp=50, alive │
│ Take lethal damage         │ isAlive=T, hp=10     │ hp<0, dead   │
│ Take damage while dead     │ isAlive=F            │ No change    │
└────────────────────────────┴──────────────────────┴──────────────┘
```

---

## 6. Collection Operations

### Pattern: Add/Remove/Contains

**Code to test**:
```csharp
public class ItemBag
{
    private List<Item> items = new List<Item>();
    
    public void Add(Item item) => items.Add(item);
    public bool Contains(Item item) => items.Contains(item);
    public int Count => items.Count;
}
```

**Test matrix**:
```
┌────────────────────────────┬────────────┬──────────────┐
│ Scenario                   │ Operation  │ Expected     │
├────────────────────────────┼────────────┼──────────────┤
│ Add single item            │ Add(x)     │ Count=1      │
│ Add multiple items         │ Add(x,y,z) │ Count=3      │
│ Contains after add         │ Contains   │ true         │
│ Contains item not added    │ Contains   │ false        │
│ Empty bag count            │ Count      │ 0            │
└────────────────────────────┴────────────┴──────────────┘
```

---

## 7. Boolean Return Coverage

### Pattern: Validation Methods

**Code to test**:
```csharp
public bool IsValidEmail(string email)
{
    if (string.IsNullOrEmpty(email))
        return false;
    
    if (!email.Contains("@"))
        return false;
    
    return true;
}
```

**Test matrix**:
```
┌────────────────────────────┬──────────────────────┬──────────┐
│ Scenario                   │ Input                │ Result   │
├────────────────────────────┼──────────────────────┼──────────┤
│ Null or empty              │ null or ""           │ false    │
│ Missing @ symbol           │ "notanemail.com"     │ false    │
│ Valid email                │ "user@domain.com"    │ true     │
└────────────────────────────┴──────────────────────┴──────────┘
```

---

## Quick Reference: Coverage Target Checklist

For each method:
- [ ] **1 test minimum** for "happy path" (normal case)
- [ ] **+1 test per branch** (if/else, switch cases)
- [ ] **+1 test per edge case** (empty, null, boundary)
- [ ] **+1 test per exception type** (catch blocks)

**Coverage formula**: (Branches covered / Total branches) × 100

**Minimum target**: 80% overall, 90% for critical methods
