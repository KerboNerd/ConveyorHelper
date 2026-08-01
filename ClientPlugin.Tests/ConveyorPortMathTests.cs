using ClientPlugin.Logic;
using NUnit.Framework;
using VRageMath;

namespace ClientPlugin.Tests;

[TestFixture]
public class ConveyorPortMathTests
{
    [TestCase("detector_conveyor", true)]
    [TestCase("detector_conveyorline_small_001", true)]
    [TestCase("detector_terminal", false)]
    [TestCase("mount", false)]
    [TestCase(null, false)]
    [TestCase("", false)]
    public void IsConveyorDummyName_filters(string name, bool expected)
    {
        Assert.AreEqual(expected, ConveyorPortMath.IsConveyorDummyName(name));
    }

    [Test]
    public void TryGetPort_plus_x_on_1x1x1()
    {
        float g = 2.5f;
        var size = Vector3I.One;
        var center = Vector3I.Zero;
        // Dummy near +X face in center-origin model space (inset 0.1m).
        var dummy = new Vector3(g * 0.5f - 0.1f, 0f, 0f);

        Assert.True(ConveyorPortMath.TryGetPort(dummy, g, size, center, Vector3.Zero, out var n, out var grid));
        Assert.That(n.X, Is.EqualTo(1f).Within(0.01f));
        Assert.That(n.Y, Is.EqualTo(0f).Within(0.01f));
        Assert.That(n.Z, Is.EqualTo(0f).Within(0.01f));
        Assert.AreEqual(Vector3I.Zero, grid);
    }

    [Test]
    public void TryGetPort_minus_y_on_1x1x1()
    {
        float g = 0.5f;
        var dummy = new Vector3(0f, -(g * 0.5f - 0.05f), 0f);

        Assert.True(ConveyorPortMath.TryGetPort(dummy, g, Vector3I.One, Vector3I.Zero, Vector3.Zero, out var n, out _));
        Assert.That(n.Y, Is.EqualTo(-1f).Within(0.01f));
    }

    [Test]
    public void TryGetPort_rejects_cell_center()
    {
        Assert.False(ConveyorPortMath.TryGetPort(Vector3.Zero, 2.5f, Vector3I.One, Vector3I.Zero, Vector3.Zero, out _, out _));
    }
}
