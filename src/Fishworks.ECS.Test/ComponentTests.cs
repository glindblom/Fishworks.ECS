using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fishworks.ECS.Test
{
  [TestClass]
  public class ComponentTests
  {

    public World world = new World();

    [TestMethod]
    public void ComponentBitmasksShouldYieldDifferentResultsForDifferentComponentTypes()
    {
      int bitmask1 = world.GetComponentBitmask<TestComponent1>();
      int bitmask2 = world.GetComponentBitmask<TestComponent2>();

      Assert.AreNotEqual(bitmask1, bitmask2);
    }

    [TestMethod]
    public void ComponentBitmasksShouldYieldSameResultsForSameComponentTypes()
    {
      int bitmask1 = world.GetComponentBitmask<TestComponent2>();
      int bitmask2 = world.GetComponentBitmask<TestComponent2>();

      int bitmask3 = world.GetComponentBitmask<TestComponent1>();
      int bitmask4 = world.GetComponentBitmask<TestComponent1>();

      Assert.AreEqual(bitmask1, bitmask2);
      Assert.AreEqual(bitmask3, bitmask4);
    }
  }

  public class TestComponent1 : IComponent
  {
  }

  public class TestComponent2 : IComponent
  {
  }
}
