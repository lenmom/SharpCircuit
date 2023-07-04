using NUnit.Framework;

using SharpCircuit;

namespace SharpCircuitTest
{

    [TestFixture]
    public class TriStateBufferTest
    {

        [TestCase(0, 0, 0, false)]
        [TestCase(0, 0, 1, false)]
        [TestCase(1, 0, 0, true)]
        [TestCase(1, 0, 1, false)]
        [TestCase(1, 1, 0, true)]
        [TestCase(1, 1, 1, true)]
        [TestCase(0, 1, 0, false)]
        [TestCase(0, 1, 1, true)]
        public void TwoToOneMuxTest(int int0, int int1, int int2, bool out0)
        {
            Circuit sim = new Circuit();

            LogicInput logicIn0 = sim.Create<LogicInput>();
            LogicInput logicIn1 = sim.Create<LogicInput>();
            LogicInput logicIn2 = sim.Create<LogicInput>();
            LogicOutput logicOut0 = sim.Create<LogicOutput>();

            TriStateBuffer tri0 = sim.Create<TriStateBuffer>();
            TriStateBuffer tri1 = sim.Create<TriStateBuffer>();

            Inverter invert0 = sim.Create<Inverter>();

            sim.Connect(logicIn0.leadOut, tri0.leadIn);
            sim.Connect(logicIn1.leadOut, tri1.leadIn);

            sim.Connect(logicIn2.leadOut, tri0.leadGate);
            sim.Connect(logicIn2.leadOut, invert0.leadIn);
            sim.Connect(invert0.leadOut, tri1.leadGate);

            sim.Connect(tri0.leadOut, tri1.leadOut);


            logicIn0.setPosition(int0);
            logicIn1.setPosition(int1);
            logicIn2.setPosition(int2);

            sim.doTicks(100);

            Debug.Log(out0, tri0.getLeadVoltage(1), tri1.getLeadVoltage(1));
            Assert.AreEqual(out0, logicOut0.isHigh());

        }

    }
}
