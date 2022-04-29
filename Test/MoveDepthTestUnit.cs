#nullable enable
using NUnit.Framework;

namespace Test
{
    
    public class MoveDepthTestUnit
    {

        private readonly MoveDepthTest Test = new();

        [Test]
        public void Depth0()
        {
            (int, int) test = Test.Depth0();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth1()
        {
            (int, int) test = Test.Depth1();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth2()
        {
            (int, int) test = Test.Depth2();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth3()
        {
            (int, int) test = Test.Depth3();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth4()
        {
            (int, int) test = Test.Depth4();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth5()
        {
            (int, int) test = Test.Depth5();
            Assert.AreEqual(test.Item1, test.Item2);
        }
        
        [Test]
        public void Depth6()
        {
            (int, int) test = Test.Depth6();
            Assert.AreEqual(test.Item1, test.Item2);
        }

    }

}