# NSubstitute Guide — Mocking & Stubbing for Unity Tests

NSubstitute is a powerful mocking library for C#. This guide covers the most common patterns for testing Unity code.

## Installation

1. In your test project, add NSubstitute via NuGet:
   ```
   PM> Install-Package NSubstitute
   ```

2. Add using statement in test files:
   ```csharp
   using NSubstitute;
   ```

## Basic Mocking

### Creating a Substitute (Mock)

```csharp
// Define interface
public interface IEnemyAI
{
    void Attack();
    bool IsAlive { get; }
}

// In test:
[Test]
public void CreateMock()
{
    var mockAI = Substitute.For<IEnemyAI>();
    // mockAI is now a fake IEnemyAI you can control
}
```

### Configuring Return Values

```csharp
var mockAI = Substitute.For<IEnemyAI>();

// Set return value for property
mockAI.IsAlive.Returns(true);

// Set return value for method call
mockAI.GetDamage().Returns(10);

// Method with parameter
mockAI.TakeDamage(Arg.Any<int>()).Returns(false);
```

### Verifying Calls

```csharp
var mockAI = Substitute.For<IEnemyAI>();

mockAI.Attack();
mockAI.Attack();

// Verify method was called exactly twice
mockAI.Received(2).Attack();

// Verify it was called at least once
mockAI.Received(1).Attack();

// Verify it was never called
var mockOther = Substitute.For<IEnemyAI>();
mockOther.Received(0).Attack();

// Or use Did...Receive
mockAI.DidNotReceive().Retreat();
```

## Advanced Patterns

### Matching Arguments

```csharp
var mockWeapon = Substitute.For<IWeapon>();

// Any argument
mockWeapon.DealDamage(Arg.Any<int>()).Returns(true);

// Specific value
mockWeapon.DealDamage(Arg.Is(50)).Returns(true);

// Argument matching predicate
mockWeapon.DealDamage(Arg.Is<int>(x => x > 10)).Returns(true);

// Multiple arguments
var mockHealth = Substitute.For<IHealthSystem>();
mockHealth.Apply(Arg.Is(DamageType.Fire), Arg.Any<int>());
```

### Callbacks (Do-When)

```csharp
// Execute custom code when method is called
var mockInventory = Substitute.For<IInventory>();

int capturedItemId = 0;
mockInventory.When(x => x.AddItem(Arg.Any<int>()))
    .Do(x => { capturedItemId = (int)x[0]; });

mockInventory.AddItem(42);
Assert.That(capturedItemId, Is.EqualTo(42));
```

### Out Parameters

```csharp
var mockParser = Substitute.For<IParser>();

// Configure out parameter
string capturedData = "";
mockParser.TryParse(Arg.Any<string>(), out Arg.Any<string>())
    .Returns(x => {
        x[1] = "parsed result";
        return true;
    });

bool result = mockParser.TryParse("input", out var data);
Assert.That(data, Is.EqualTo("parsed result"));
```

### Raising Events

```csharp
public interface IEventPublisher
{
    event EventHandler OnEvent;
    void Publish();
}

[Test]
public void EventRaised_SubscriberNotified()
{
    var mockPublisher = Substitute.For<IEventPublisher>();
    
    bool eventRaised = false;
    mockPublisher.OnEvent += (s, e) => { eventRaised = true; };
    
    // Simulate event
    mockPublisher.OnEvent += Raise.Event<EventHandler>(this, EventArgs.Empty);
    
    Assert.That(eventRaised, Is.True);
}
```

## Real-World Examples

### Testing a Service

```csharp
public interface IPlayerRepository
{
    Player GetPlayer(int id);
}

public class PlayerService
{
    private IPlayerRepository repo;
    
    public PlayerService(IPlayerRepository repository)
    {
        repo = repository;
    }
    
    public void LevelUp(int playerId)
    {
        var player = repo.GetPlayer(playerId);
        if (player != null)
            player.Level++;
    }
}

[Test]
public void LevelUp_FetchesAndUpdates()
{
    var mockRepo = Substitute.For<IPlayerRepository>();
    var player = new Player { Id = 1, Level = 1 };
    mockRepo.GetPlayer(1).Returns(player);
    
    var service = new PlayerService(mockRepo);
    service.LevelUp(1);
    
    Assert.That(player.Level, Is.EqualTo(2));
    mockRepo.Received(1).GetPlayer(1);
}
```

### Testing a Manager

```csharp
public interface ISpawner
{
    void SpawnEnemy(Vector3 position);
}

public class EnemyManager
{
    private ISpawner spawner;
    private List<IEnemy> activeEnemies;
    
    public EnemyManager(ISpawner spawnService)
    {
        spawner = spawnService;
        activeEnemies = new List<IEnemy>();
    }
    
    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            spawner.SpawnEnemy(Vector3.zero);
        }
    }
}

[Test]
public void SpawnWave_SpawnsCorrectCount()
{
    var mockSpawner = Substitute.For<ISpawner>();
    var manager = new EnemyManager(mockSpawner);
    
    manager.SpawnWave(5);
    
    mockSpawner.Received(5).SpawnEnemy(Arg.Any<Vector3>());
}
```

### Testing Multiple Calls with Different Returns

```csharp
public interface ILootTable
{
    int GetRandomReward();
}

public class LootSystem
{
    private ILootTable table;
    
    public LootSystem(ILootTable lootTable)
    {
        table = lootTable;
    }
    
    public List<int> RollMultiple(int count)
    {
        var rewards = new List<int>();
        for (int i = 0; i < count; i++)
            rewards.Add(table.GetRandomReward());
        return rewards;
    }
}

[Test]
public void RollMultiple_ReturnsSequence()
{
    var mockTable = Substitute.For<ILootTable>();
    
    // Return different values on successive calls
    mockTable.GetRandomReward()
        .Returns(10, 20, 30); // First call returns 10, second 20, third 30
    
    var loot = new LootSystem(mockTable);
    var results = loot.RollMultiple(3);
    
    Assert.That(results, Is.EqualTo(new[] { 10, 20, 30 }));
}
```

## Comparison to Other Approaches

| Approach | Pros | Cons |
|----------|------|------|
| **NSubstitute** | Easy syntax, great for interfaces | Requires interface extraction |
| **Moq** | Popular, similar to NSubstitute | More verbose in some cases |
| **Hand-written mocks** | Full control, no dependencies | Tedious, error-prone |
| **Dependency Injection container** | Flexible, composable | Overkill for small tests |

## Common Pitfalls

### Forgetting to Configure Returns

```csharp
// ❌ Wrong: Returns null by default
var mock = Substitute.For<IService>();
var result = mock.GetValue(); // result is null

// ✅ Correct: Explicitly set return
mock.GetValue().Returns(42);
var result = mock.GetValue(); // result is 42
```

### Mixing Real and Mock Objects

```csharp
// ❌ Wrong: Mixing real and fake
var mock = Substitute.For<IRepository>();
var realService = new MyService(mock); // real service, fake repo → works
var result = realService.DoWork(); // unpredictable behavior

// ✅ Correct: All controlled
var mock = Substitute.For<IRepository>();
mock.GetData().Returns(testData);
var service = new MyService(mock);
var result = service.DoWork(); // predictable
```

### Not Verifying Calls

```csharp
// ❌ Wrong: Only checking return value
var mock = Substitute.For<IService>();
mock.DoWork().Returns(true);
var result = mock.DoWork();
Assert.That(result, Is.True); // Doesn't verify DoWork was actually called

// ✅ Correct: Verify interaction
mock.Received(1).DoWork();
```

## Cheat Sheet

| Task | Code |
|------|------|
| Create mock | `Substitute.For<IInterface>()` |
| Set return | `.Returns(value)` |
| Verify called | `.Received(n).Method()` |
| Verify never called | `.DidNotReceive().Method()` |
| Any argument | `Arg.Any<T>()` |
| Specific value | `Arg.Is(value)` |
| Match predicate | `Arg.Is<T>(x => condition)` |
| Out parameter | `.Returns(x => { x[1] = value; return result; })` |
| Raise event | `mockPublisher.OnEvent += Raise.Event<Handler>()` |
| Sequence returns | `.Returns(val1, val2, val3)` |
