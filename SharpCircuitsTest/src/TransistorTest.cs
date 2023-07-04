using System;

using NUnit.Framework;

using SharpCircuit;

namespace SharpCircuitTest
{

    [TestFixture]
    public class TransistorTest
    {

        [Test]
        public void NPNTransistorTest()
        {
            Circuit sim = new Circuit();

            Transistor npn0 = sim.Create<Transistor>(false);

            VoltageInput baseVoltage = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            baseVoltage.maxVoltage = 0.7025;

            VoltageInput collectorVoltage = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            collectorVoltage.maxVoltage = 2;

            Ground ground = sim.Create<Ground>();

            Wire baseWire = sim.Create<Wire>();
            Wire collectorWire = sim.Create<Wire>();
            Wire emitterWire = sim.Create<Wire>();

            sim.Connect(baseVoltage.leadPos, baseWire.leadIn);
            sim.Connect(baseWire.leadOut, npn0.leadBase);

            sim.Connect(collectorVoltage.leadPos, collectorWire.leadIn);
            sim.Connect(collectorWire.leadOut, npn0.leadCollector);

            sim.Connect(ground.leadIn, emitterWire.leadIn);
            sim.Connect(emitterWire.leadOut, npn0.leadEmitter);

            sim.doTicks(100);

            TestUtils.Compare(baseWire.getCurrent(), 0.00158254, 8);
            TestUtils.Compare(collectorWire.getCurrent(), 0.15825359, 8);
            TestUtils.Compare(emitterWire.getCurrent(), -0.15983612, 8);
        }

        [Test]
        public void PNPTransistorTest()
        {
            Circuit sim = new Circuit();

            Transistor pnp0 = sim.Create<Transistor>(true);

            VoltageInput baseVoltage = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            baseVoltage.maxVoltage = 1.3;

            VoltageInput collectorVoltage = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            collectorVoltage.maxVoltage = 2;

            VoltageInput emitterVoltage = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            emitterVoltage.maxVoltage = 2;

            Wire baseWire = sim.Create<Wire>();
            Wire collectorWire = sim.Create<Wire>();
            Wire emitterWire = sim.Create<Wire>();

            sim.Connect(baseVoltage.leadPos, baseWire.leadIn);
            sim.Connect(baseWire.leadOut, pnp0.leadBase);

            sim.Connect(collectorVoltage.leadPos, collectorWire.leadIn);
            sim.Connect(collectorWire.leadOut, pnp0.leadCollector);

            sim.Connect(emitterVoltage.leadPos, emitterWire.leadIn);
            sim.Connect(emitterWire.leadOut, pnp0.leadEmitter);

            sim.doTicks(100);

            TestUtils.Compare(baseWire.getCurrent(), -0.07374479, 8);
            TestUtils.Compare(collectorWire.getCurrent(), 0.00143194, 8);
            TestUtils.Compare(emitterWire.getCurrent(), 0.07231284, 8);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SwitchTest(bool In0)
        {
            Circuit sim = new Circuit();

            DCVoltageSource volt0 = sim.Create<DCVoltageSource>();

            Resistor res0 = sim.Create<Resistor>(10000);
            Resistor res1 = sim.Create<Resistor>(300);

            SwitchSPST switch0 = sim.Create<SwitchSPST>();

            Transistor npn0 = sim.Create<Transistor>(false);

            sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(res0.leadOut, volt0.leadNeg);

            /*sim.Connect(volt0.leadPos, res0.leadIn);
			sim.Connect(volt0.leadPos, res1.leadIn);

			sim.Connect(switch0, 0, res0, 1);
			sim.Connect(switch0, 1, npn0, 0);

			sim.Connect(res1.leadOut, npn0.leadCollector);
			sim.Connect(npn0.leadEmitter, volt0.leadNeg);*/

            //if(In0) switch0.toggle();

            sim.doTicks(100);

            //Debug.Log(res0.getVoltageDiff());
            //Debug.Log(res0.getCurrent());
            //Debug.Log(res1.getVoltageDiff());
            //Debug.Log(res1.getCurrent());

            //Debug.Log(volt0.getLeadVoltage(1));

            //Debug.Log();
            //Debug.Log(res0.getLeadVoltage(0));
            //Debug.Log(res0.getLeadVoltage(1));

            //Debug.Log();
            //Debug.Log(res1.getLeadVoltage(0));
            //Debug.Log(res1.getLeadVoltage(1));

            //Debug.Log();
            //Debug.Log(switch0.getLeadVoltage(0));
            //Debug.Log(switch0.getLeadVoltage(1));
            Assert.Ignore();
        }

        [Test]
        public void CurrentSourceRampTest()
        {
            Assert.Ignore("Not Implemented!");
        }

        [TestCase(true, 5.2E-10)]
        [TestCase(false, 0.014121382555)]
        public void DarlingtonPairTest(bool In0, double i0)
        {
            Circuit sim = new Circuit();

            VoltageInput volt0 = sim.Create<VoltageInput>();

            Resistor res0 = sim.Create<Resistor>(2000000);
            Resistor res1 = sim.Create<Resistor>(300);

            SwitchSPST switch0 = sim.Create<SwitchSPST>();

            Transistor npn0 = sim.Create<Transistor>(false);
            Transistor npn1 = sim.Create<Transistor>(false);

            Ground groun0 = sim.Create<Ground>();

            sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(volt0.leadPos, res1.leadIn);

            sim.Connect(switch0, 0, res0, 1);

            sim.Connect(npn0, 0, switch0, 1);
            sim.Connect(npn0.leadCollector, res1.leadOut);
            sim.Connect(npn0.leadEmitter, npn1.leadBase);

            sim.Connect(npn1.leadCollector, res1.leadOut);
            sim.Connect(npn1.leadEmitter, groun0.leadIn);

            if (In0)
            {
                switch0.toggle();
            }

            sim.doTicks(100);

            Assert.AreEqual(i0, Math.Round(groun0.getCurrent(), 12));
        }

    }
}
