using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fishworks.ECS.Test
{
  [TestClass]
  public class SystemTests
  {
    [TestMethod]
    public void SystemComponentInterestsWorkingPositive()
    {
      TestSystem testSystem = new TestSystem(new World());

      Assert.AreEqual(testSystem.InterestedIn(new Type[] { typeof(TestComponent1), typeof(TestComponent2) }), true);
    }

    [TestMethod]
    public void SystemComponentInterestsWorkingNegative()
    {
      TestSystem testSystem = new TestSystem(new World());

      Assert.AreEqual(testSystem.InterestedIn(new Type[] { typeof(TestComponent1), typeof(TestComponent3) }), false);
    }

    [TestMethod]
    public void SystemAddingCompositionsAsIntended()
    {
      var world = new World();
      var system = new TestSystem(world);

      var entity1 = world.CreateEntity()
                    .AddComponent<TestComponent1>()
                    .AddComponent<TestComponent2>()
                    .AddToWorld();

      var entity2 = world.CreateEntity()
                    .AddComponent<TestComponent1>()
                    .AddComponent<TestComponent2>()
                    .AddToWorld();

      var entity3 = world.CreateEntity()
        .AddComponent<TestComponent1>()
        .AddToWorld();

      Assert.AreEqual(2, system.EntityCount);
    }

    [TestMethod]
    public void SystemCompositionsWorkingAsIntended()
    {
      var world = new World();
      var system = new TestSystem(world);

      var entity1 = world.CreateEntity()
                    .AddComponent<TestComponent1>()
                    .AddComponent<TestComponent2>()
                    .AddToWorld();

      Assert.IsNotNull(system.GetComposition.TestComponent1);
      Assert.IsNotNull(system.GetComposition.TestComponent2);
    }
  }

  public class TestComponent3 : IComponent
  {
    
  }

  public class TestSystem : BaseSystem
  {
    public TestSystem(World world) : base(world, new Type[] { typeof(TestComponent1), typeof(TestComponent2) })
    {

    }

    public bool InterestedIn(Type[] componentTypes)
    {
      int bitmask = 0;
      int i = 1;
      foreach (var componentType in componentTypes)
      {
        bitmask |= World.GetComponentBitmask(componentType);
        i++;
      }

      return (SystemBitmask & bitmask) == SystemBitmask;
    }

    public int EntityCount => Compositions.Keys.Count;
    public override void Update(float deltaTime)
    {
      throw new NotImplementedException();
    }

    public override void ProcessEntity(dynamic entityComposition)
    {
      throw new NotImplementedException();
    }

    public dynamic GetComposition => Compositions.Values.ElementAt(0);
  }
}
