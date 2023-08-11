using SimpleVM;

namespace SimpleVMTests;

public class LineTrackerTests
{
    [Test]
    public void TestLineTracker()
    {
        var tracker = new LineTracker();

        for (uint i = 0; i < 16; i++)
        {
            tracker.AddLine(i, (LineNumber) (i / 3));
        }

        for (var i = 0; i < 16; i++)
        {
            Console.WriteLine(i + "  " + tracker.GetLineNumber(i));
        }

        for (int i = 0; i < tracker.Datas.Count; i++)
        {
            Console.WriteLine(tracker.Datas[i].Offset + "  nr  " + tracker.Datas[i].LineNumber);
            //Console.WriteLine(tracker.Datas[i].LineNumber);
        }
        https://craftinginterpreters.com/a-virtual-machine.html

        //var nr=tracker.Datas[i]

        Console.WriteLine(tracker.GetLineNumber(7));
    }
}