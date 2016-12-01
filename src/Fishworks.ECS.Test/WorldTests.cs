using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fishworks.ECS.Test
{
  [TestClass]
  public class WorldTests
  {
    [TestMethod]
    public void WorldShouldIncrementSize()
    {
      var world = new World();
      for (int i = 0; i < 1000; i++)
      {
        world.CreateEntity()
          .AddToWorld();
      }

      Assert.AreEqual(1000, world.EntityCount);
    }

    [TestMethod]
    public void WorldShouldCreateEntitiesFromRemovedIds()
    {
      var world = new World();
      for (int i = 0; i < 1000; i++)
      {
        world.CreateEntity()
          .AddToWorld();
      }

      int idToRemove1 = 128;
      int idToRemove2 = 455;

      world.DestroyEntity((uint)idToRemove1);
      world.DestroyEntity((uint)idToRemove2);

      var newEntity1 = world.CreateEntity()
        .AddComponent(new TestComponent1())
        .AddToWorld();

      var newEntity2 = world.CreateEntity()
        .AddComponent(new TestComponent2())
        .AddToWorld();

      Assert.AreEqual<uint>((uint)idToRemove1, newEntity1.Id);
      Assert.AreEqual<uint>((uint)idToRemove2, newEntity2.Id);
    }

    [TestMethod]
    public void ComponentBitmasksWorkingAsIntendedInWorld()
    {
      var world = new World();

      int bitmask1 = world.GetComponentBitmask<EntityStatusComponent>();
      int bitmask2 = world.GetComponentBitmask<TestComponent1>();
      int bitmask3 = world.GetComponentBitmask<TestComponent2>();

      Trace.WriteLine("ComponentBitmasks: " + bitmask1 + " " + bitmask2 + " " + bitmask3);

      Assert.AreNotEqual(bitmask1, bitmask2);
      Assert.AreNotEqual(bitmask1, bitmask3);
      Assert.AreNotEqual(bitmask2, bitmask3);
    }
  }
}
