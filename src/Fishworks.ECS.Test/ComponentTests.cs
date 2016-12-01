using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fishworks.ECS.Test
{
  [TestClass]
  public class ComponentTests
  {
    [TestMethod]
    public void ComponentBitmasksShouldYieldDifferentResultsForDifferentComponentTypes()
    {
      int bitmask1 = typeof (TestComponent1).GetComponentBitmask();
      int bitmask2 = typeof (TestComponent2).GetComponentBitmask();

      Assert.AreNotEqual(bitmask1, bitmask2);
    }

    [TestMethod]
    public void ComponentBitmasksShouldYieldSameResultsForSameComponentTypes()
    {
      int bitmask1 = typeof (TestComponent2).GetComponentBitmask();
      int bitmask2 = typeof (TestComponent2).GetComponentBitmask();

      int bitmask3 = typeof (TestComponent1).GetComponentBitmask();
      int bitmask4 = typeof (TestComponent1).GetComponentBitmask();

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
