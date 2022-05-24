using Backend.Data.Move;
using NUnit.Framework;

namespace Test;

public class Perft
{

    private readonly Backend.Perft Test = new();
        
    [SetUp]
    public void SetUp()
    {
        AttackTable.SetUp();
    }

    [Test]
    public void Depth1()
    {
        (ulong, ulong) test = Test.Depth1();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth2()
    {
        (ulong, ulong) test = Test.Depth2();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth3()
    {
        (ulong, ulong) test = Test.Depth3();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth4()
    {
        (ulong, ulong) test = Test.Depth4();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth5()
    {
        (ulong, ulong) test = Test.Depth5();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth6()
    {
        (ulong, ulong) test = Test.Depth6();
        Assert.AreEqual(test.Item1, test.Item2);
    }
        
    [Test]
    public void Depth7()
    {
        (ulong, ulong) test = Test.Depth7();
        Assert.AreEqual(test.Item1, test.Item2);
    }

}