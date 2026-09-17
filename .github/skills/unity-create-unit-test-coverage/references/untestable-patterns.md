# Untestable Code Patterns — Solutions

This guide addresses hard-to-test Unity patterns and provides refactoring strategies and test approaches.

---

## 1. Singletons

### The Problem

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }
    
    public void DoSomething() { ... }
}

// Usage in other classes:
GameManager.Instance.DoSomething(); // ← Hard to mock, hard to reset
```

**Why it's hard to test**:
- Can't inject a mock version
- Persists across tests
- Requires scene setup
- Difficult to reset state

### Solution 1: Extract Interface + Inject

```csharp
// Define interface
public interface IGameManager
{
    void DoSomething();
}

// Implement singleton (keeping it for existing code)
public class GameManager : MonoBehaviour, IGameManager
{
    public static GameManager Instance { get; private set; }
    // ... existing code ...
}

// In classes needing GameManager, accept dependency:
public class Player
{
    private IGameManager gameManager;
    
    public Player(IGameManager manager)
    {
        gameManager = manager; // Can inject mock in tests
    }
    
    public void TakeDamage()
    {
        gameManager.DoSomething();
    }
}
```

**Test**:
```csharp
[Test]
public void TakeDamage_CallsGameManager()
{
    var mockManager = Substitute.For<IGameManager>();
    var player = new Player(mockManager);
    
    player.TakeDamage();
    
    mockManager.Received(1).DoSomething();
}
```

### Solution 2: Service Locator (Temporary)

```csharp
// Register during game startup
public static class ServiceRegistry
{
    private static Dictionary<Type, object> services = new();
    
    public static void Register<T>(T service) => services[typeof(T)] = service;
    public static T Get<T>() => (T)services[typeof(T)];
}

// In test setup:
[SetUp]
public void Setup()
{
    var mockManager = Substitute.For<IGameManager>();
    ServiceRegistry.Register(mockManager);
}

[Test]
public void TestSomething()
{
    var manager = ServiceRegistry.Get<IGameManager>();
    // Now use the mock
}
```

---

## 2. Static Methods

### The Problem

```csharp
public class MathUtils
{
    public static float Distance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b); // ← Can't mock
    }
}

public class Player
{
    public void MoveTo(Vector3 target)
    {
        float dist = MathUtils.Distance(transform.position, target);
        if (dist > 0.1f)
            Rigidbody.velocity = GetVelocity();
    }
}
```

**Why it's hard to test**:
- Can't substitute a mock
- Tests use real implementation
- Can't control return values
- Difficult to verify calls

### Solution: Wrapper Interface + Dependency Injection

```csharp
// Define interface
public interface IMathUtils
{
    float Distance(Vector3 a, Vector3 b);
}

// Static wrapper (for backward compatibility)
public class MathUtils : IMathUtils
{
    public static IMathUtils Instance { get; } = new MathUtils();
    
    public float Distance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }
}

// Refactor Player to use dependency
public class Player
{
    private readonly IMathUtils mathUtils;
    
    public Player(IMathUtils utils)
    {
        mathUtils = utils;
    }
    
    public void MoveTo(Vector3 target)
    {
        float dist = mathUtils.Distance(transform.position, target);
        // ... rest of code ...
    }
}
```

**Test**:
```csharp
[Test]
public void MoveTo_WhenDistanceSmall_DoesNotMove()
{
    var mockMath = Substitute.For<IMathUtils>();
    mockMath.Distance(Arg.Any<Vector3>(), Arg.Any<Vector3>()).Returns(0.05f);
    
    var player = new Player(mockMath);
    player.MoveTo(new Vector3(10, 0, 0));
    
    // Verify movement didn't happen
    Assert.That(player.GetComponent<Rigidbody>().velocity, Is.EqualTo(Vector3.zero));
}
```

---

## 3. MonoBehaviour.GetComponent<>()

### The Problem

```csharp
public class EnemyController : MonoBehaviour
{
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // ← Requires GameObject + scene
    }
    
    public void Push(Vector3 force)
    {
        rb.AddForce(force);
    }
}
```

**Why it's hard to test**:
- Requires MonoBehaviour instantiation in scene
- GetComponent() returns null in Play Mode tests unless configured
- Slower than unit tests (Play Mode only)

### Solution 1: Constructor Injection

```csharp
public class EnemyController : MonoBehaviour
{
    private Rigidbody rb;
    
    // Public constructor for testing
    public EnemyController(Rigidbody rigidbody)
    {
        rb = rigidbody;
    }
    
    private void Awake()
    {
        if (rb == null) // Fallback for scene instantiation
            rb = GetComponent<Rigidbody>();
    }
    
    public void Push(Vector3 force)
    {
        rb.AddForce(force);
    }
}
```

**Test** (Edit Mode, no scene needed):
```csharp
[Test]
public void Push_AppliesForce()
{
    var mockRb = Substitute.For<Rigidbody>();
    var controller = new EnemyController(mockRb);
    
    controller.Push(Vector3.forward * 10f);
    
    mockRb.Received(1).AddForce(Arg.Is<Vector3>(v => v == Vector3.forward * 10f));
}
```

### Solution 2: Play Mode Test with Test Scene

```csharp
// In Assets/Tests/PlayModeTests/EnemyControllerTests.cs
public class EnemyControllerPlayModeTests
{
    private GameObject enemyGO;
    
    [OneTimeSetUp]
    public void LoadTestScene()
    {
        // Unity loads scenes automatically if in PlayModeTests folder
        // Otherwise, manually load with SceneManager.LoadScene("TestScene");
    }
    
    [SetUp]
    public void Setup()
    {
        enemyGO = new GameObject("TestEnemy");
        enemyGO.AddComponent<Rigidbody>();
        enemyGO.AddComponent<EnemyController>();
    }
    
    [TearDown]
    public void Cleanup()
    {
        Object.Destroy(enemyGO);
    }
    
    [UnityTest]
    public IEnumerator Push_AppliesForce()
    {
        var controller = enemyGO.GetComponent<EnemyController>();
        var rb = enemyGO.GetComponent<Rigidbody>();
        
        controller.Push(Vector3.forward * 10f);
        
        yield return null; // Let physics update
        Assert.That(rb.velocity.magnitude, Is.GreaterThan(0));
    }
}
```

---

## 4. Time.deltaTime

### The Problem

```csharp
public class Timer
{
    private float elapsed = 0f;
    
    public void Tick()
    {
        elapsed += Time.deltaTime; // ← Non-deterministic
    }
    
    public float GetElapsed() => elapsed;
}
```

**Why it's hard to test**:
- Time.deltaTime varies per frame (non-deterministic)
- Can't control time progression in tests
- Timing-dependent tests are flaky

### Solution: Inject Time Provider

```csharp
// Define time interface
public interface ITimeProvider
{
    float DeltaTime { get; }
    float Time { get; }
}

// Concrete implementation
public class UnityTimeProvider : ITimeProvider
{
    public float DeltaTime => UnityEngine.Time.deltaTime;
    public float Time => UnityEngine.Time.time;
}

// Refactor Timer
public class Timer
{
    private float elapsed = 0f;
    private ITimeProvider timeProvider;
    
    public Timer(ITimeProvider provider)
    {
        timeProvider = provider;
    }
    
    public void Tick()
    {
        elapsed += timeProvider.DeltaTime;
    }
    
    public float GetElapsed() => elapsed;
}
```

**Test** (deterministic):
```csharp
[Test]
public void Tick_IncrementsElapsedTime()
{
    var mockTime = Substitute.For<ITimeProvider>();
    mockTime.DeltaTime.Returns(0.016f); // 60 FPS
    
    var timer = new Timer(mockTime);
    
    timer.Tick();
    timer.Tick();
    timer.Tick();
    
    Assert.That(timer.GetElapsed(), Is.EqualTo(0.048f).Within(0.001f));
}
```

---

## 5. Physics.Raycast

### The Problem

```csharp
public class WeaponController : MonoBehaviour
{
    public bool TryHit(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f))
        {
            var target = hit.collider.GetComponent<IDamageable>();
            target?.TakeDamage(10);
            return true;
        }
        return false;
    }
}
```

**Why it's hard to test**:
- Physics.Raycast requires colliders in scene
- Non-deterministic (depends on scene geometry)
- Play Mode test only
- Slow

### Solution: Inject Physics Provider

```csharp
// Define interface
public interface IPhysicsProvider
{
    bool TryRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance);
}

// Real implementation
public class UnityPhysicsProvider : IPhysicsProvider
{
    public bool TryRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance)
    {
        return Physics.Raycast(origin, direction, out hit, maxDistance);
    }
}

// Refactor WeaponController
public class WeaponController
{
    private IPhysicsProvider physics;
    
    public WeaponController(IPhysicsProvider physicsProvider)
    {
        physics = physicsProvider;
    }
    
    public bool TryHit(Vector3 origin, Vector3 direction)
    {
        if (physics.TryRaycast(origin, direction, out RaycastHit hit, 100f))
        {
            var target = hit.collider.GetComponent<IDamageable>();
            target?.TakeDamage(10);
            return true;
        }
        return false;
    }
}
```

**Test** (Edit Mode, no scene):
```csharp
[Test]
public void TryHit_WhenHitDetected_DamagesTarget()
{
    var mockPhysics = Substitute.For<IPhysicsProvider>();
    var mockCollider = Substitute.For<Collider>();
    var mockDamageable = Substitute.For<IDamageable>();
    
    // Simulate a hit
    var hit = new RaycastHit { collider = mockCollider };
    mockPhysics.TryRaycast(Arg.Any<Vector3>(), Arg.Any<Vector3>(), out Arg.Any<RaycastHit>(), Arg.Any<float>())
        .Returns(x => { x[2] = hit; return true; });
    
    mockCollider.GetComponent<IDamageable>().Returns(mockDamageable);
    
    var weapon = new WeaponController(mockPhysics);
    bool result = weapon.TryHit(Vector3.zero, Vector3.forward);
    
    Assert.That(result, Is.True);
    mockDamageable.Received(1).TakeDamage(10);
}
```

---

## 6. Coroutines

### The Problem

```csharp
public class DialogManager : MonoBehaviour
{
    public void ShowDialog(string text)
    {
        StartCoroutine(DisplayText(text));
    }
    
    private IEnumerator DisplayText(string text)
    {
        uiText.text = text;
        yield return new WaitForSeconds(2f);
        uiText.text = "";
    }
}
```

**Why it's hard to test**:
- Must use Play Mode tests
- Time-dependent (slow tests)
- StartCoroutine() only works on MonoBehaviour

### Solution 1: Extract Logic to Testable Method

```csharp
public class DialogManager : MonoBehaviour
{
    public void ShowDialog(string text)
    {
        StartCoroutine(DisplayText(text));
    }
    
    // Testable method (no coroutine)
    public string GetDialogText(float elapsedTime)
    {
        if (elapsedTime < 2f)
            return text;
        else
            return "";
    }
    
    private IEnumerator DisplayText(string text)
    {
        uiText.text = text;
        yield return new WaitForSeconds(2f);
        uiText.text = GetDialogText(2f);
    }
}
```

**Test** (Edit Mode):
```csharp
[Test]
public void GetDialogText_AtTwoSeconds_ReturnsEmpty()
{
    var manager = new DialogManager();
    string result = manager.GetDialogText(2.1f);
    Assert.That(result, Is.EqualTo(""));
}
```

### Solution 2: Play Mode Test with Manual Coroutine

```csharp
public class DialogManagerPlayModeTests
{
    [UnityTest]
    public IEnumerator ShowDialog_WaitsAndClears()
    {
        var go = new GameObject();
        var manager = go.AddComponent<DialogManager>();
        
        manager.ShowDialog("Test");
        
        // Wait for coroutine to complete
        yield return new WaitForSeconds(2.5f);
        
        Assert.That(manager.uiText.text, Is.EqualTo(""));
        
        Object.Destroy(go);
    }
}
```

---

## 7. Scene References

### The Problem

```csharp
public class SpawnManager : MonoBehaviour
{
    private Transform spawnPoint;
    
    private void Start()
    {
        spawnPoint = GameObject.Find("SpawnPoint").transform; // ← Scene-dependent
    }
    
    public void Spawn(GameObject prefab)
    {
        Instantiate(prefab, spawnPoint);
    }
}
```

**Why it's hard to test**:
- Requires specific scene hierarchy
- GameObject.Find() returns null if not in scene
- Must use Play Mode tests

### Solution: Constructor Injection

```csharp
public class SpawnManager : MonoBehaviour
{
    private Transform spawnPoint;
    
    // Constructor for testing
    public SpawnManager(Transform spawn)
    {
        spawnPoint = spawn;
    }
    
    private void Start()
    {
        if (spawnPoint == null) // Fallback
            spawnPoint = GameObject.Find("SpawnPoint").transform;
    }
    
    public void Spawn(GameObject prefab)
    {
        Instantiate(prefab, spawnPoint);
    }
}
```

**Test** (Edit Mode):
```csharp
[Test]
public void Spawn_InstantiatesAtSpawnPoint()
{
    var spawnGO = new GameObject("SpawnPoint");
    var manager = new SpawnManager(spawnGO.transform);
    
    var prefab = Resources.Load<GameObject>("TestPrefab");
    manager.Spawn(prefab);
    
    // Verify instantiation (use FindObjectOfType or similar)
    Assert.That(Object.FindObjectOfType<YourPrefabType>(), Is.Not.Null);
    
    Object.Destroy(spawnGO);
}
```

---

## 8. Events & UnityEvents

### The Problem

```csharp
public class HealthSystem : MonoBehaviour
{
    public UnityEvent<int> OnHealthChanged;
    
    public void TakeDamage(int amount)
    {
        health -= amount;
        OnHealthChanged?.Invoke(health); // ← Hard to verify
    }
}
```

**Why it's hard to test**:
- UnityEvent is not easily mockable
- Difficult to assert on event calls

### Solution: Define Interface + Use Substitute

```csharp
// Define event interface
public interface IHealthChangeListener
{
    void OnHealthChanged(int newHealth);
}

public class HealthSystem
{
    private int health = 100;
    private IHealthChangeListener listener;
    
    public HealthSystem(IHealthChangeListener changeListener)
    {
        listener = changeListener;
    }
    
    public void TakeDamage(int amount)
    {
        health -= amount;
        listener?.OnHealthChanged(health);
    }
}
```

**Test**:
```csharp
[Test]
public void TakeDamage_InvokesHealthChangeListener()
{
    var listener = Substitute.For<IHealthChangeListener>();
    var health = new HealthSystem(listener);
    
    health.TakeDamage(10);
    
    listener.Received(1).OnHealthChanged(90);
}
```

---

## Refactoring Strategy Checklist

When faced with untestable code:

1. **Identify the dependency**: What can't be controlled (Singleton, static method, scene, etc.)?
2. **Extract an interface**: Define a mockable contract.
3. **Inject via constructor**: Depend on abstraction, not concretion.
4. **Keep backward compatibility**: Often the scene/old code can use the old way.
5. **Test with mocks**: Use NSubstitute or similar.
6. **Verify side effects**: Assert on Received() calls and state changes.

**Golden rule**: Testable code is usually better-designed code.
