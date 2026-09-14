using NUnit.Framework;

namespace Ftg.SoundSystem.Tests
{
    public sealed class SoundMathTests
    {
        [TestCase(1f, 0f)]
        [TestCase(0.1f, -20f)]
        [TestCase(0.01f, -40f)]
        public void LinearToDecibels_ConvertsExpectedValues(float linear, float expected)
        {
            Assert.That(SoundMath.LinearToDecibels(linear), Is.EqualTo(expected).Within(0.001f));
        }

        [Test]
        public void LinearToDecibels_ZeroReturnsSilence()
        {
            Assert.That(SoundMath.LinearToDecibels(0f), Is.EqualTo(SoundMath.SilenceDecibels));
        }
    }
}
