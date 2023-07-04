using System;

using NUnit.Framework;

using SharpCircuit;

namespace SharpCircuitTest.Logic
{

    [TestFixture]
    public class NMOSTest
    {

        [TestCase(true, false, 0.001)]
        [TestCase(false, true, 0)]
        public void InverterTest(bool in0, bool in1, double in2)
        {
            Circuit sim = new Circuit();

            VoltageInput volt0 = sim.Create<VoltageInput>();
            Resistor res0 = sim.Create<Resistor>(5000);
            NMosfet nmos0 = sim.Create<NMosfet>();
            LogicInput logicIn0 = sim.Create<LogicInput>();
            LogicOutput logicOut0 = sim.Create<LogicOutput>();
            Ground grnd0 = sim.Create<Ground>();

            sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(res0.leadOut, nmos0.leadDrain);
            sim.Connect(logicIn0.leadOut, nmos0.leadGate);
            sim.Connect(logicOut0.leadIn, nmos0.leadDrain);
            sim.Connect(nmos0.leadSrc, grnd0.leadIn);

            if (in0)
            {
                logicIn0.toggle();
            }

            sim.doTicks(100);

            Assert.AreEqual(in1, logicOut0.isHigh());
            Assert.AreEqual(in2, Math.Round(grnd0.getCurrent(), 3));
        }

        [TestCase(false, false, true, 0)]
        [TestCase(true, false, true, 0)]
        [TestCase(false, true, true, 0)]
        [TestCase(true, true, false, 0.001)]
        public void NANDTest(bool in0, bool in1, bool in2, double in3)
        {
            Circuit sim = new Circuit();

            VoltageInput volt0 = sim.Create<VoltageInput>();
            Resistor res0 = sim.Create<Resistor>(5000);
            NMosfet nmos0 = sim.Create<NMosfet>();
            NMosfet nmos1 = sim.Create<NMosfet>();
            LogicInput logicIn0 = sim.Create<LogicInput>();
            LogicInput logicIn1 = sim.Create<LogicInput>();
            LogicOutput logicOut0 = sim.Create<LogicOutput>();
            Ground grnd0 = sim.Create<Ground>();

            sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(res0.leadOut, nmos0.leadDrain);

            sim.Connect(logicOut0.leadIn, nmos0.leadDrain);
            sim.Connect(nmos0.leadGate, logicIn0.leadOut);

            sim.Connect(nmos0.leadSrc, nmos1.leadDrain);

            sim.Connect(nmos1.leadGate, logicIn1.leadOut);
            sim.Connect(nmos1.leadSrc, grnd0.leadIn);

            if (in0)
            {
                logicIn0.toggle();
            }

            if (in1)
            {
                logicIn1.toggle();
            }

            sim.doTicks(100);

            Assert.AreEqual(in2, logicOut0.isHigh());
            Assert.AreEqual(in3, Math.Round(grnd0.getCurrent(), 3));
        }

    }
}
