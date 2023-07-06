using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NUnit.Framework;

using SharpCircuit;

namespace SharpCircuitTest
{

    [TestFixture]
    public class FluidTest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="clockwise">if the control circute is reverse direction.</param>
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void SwitchSPSTTest(bool totalEnable, bool clockwise)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.AC);
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            SwitchSPST switchA = sim.Create<SwitchSPST>(true);
            SwitchSPST switchB = sim.Create<SwitchSPST>(true);
            SwitchSPST switchC = sim.Create<SwitchSPST>(true);
            SwitchSPST switchD = sim.Create<SwitchSPST>(true);

            Resistor resistor = sim.Create<Resistor>();

            Ground grndTop = sim.Create<Ground>();
            Ground grndBottom = sim.Create<Ground>();

            LogicOutput logicOutTop = sim.Create<LogicOutput>();
            LogicOutput logicOutBottom = sim.Create<LogicOutput>();

            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, switchB.leadA);
            sim.Connect(switchTotal.leadB, switchC.leadA);

            sim.Connect(switchA.leadB, grndTop.leadIn);
            sim.Connect(switchA.leadA, resistor.leadOut);

            sim.Connect(switchB.leadB, resistor.leadOut);

            sim.Connect(switchC.leadB, resistor.leadIn);

            sim.Connect(switchD.leadB, grndBottom.leadIn);
            sim.Connect(switchD.leadA, resistor.leadIn);

            sim.Connect(logicOutTop.leadIn, resistor.leadOut);
            sim.Connect(logicOutBottom.leadIn, resistor.leadIn);

            /*
               1. if switchTotal closed, then the whole circute is connected; otherwise, disconnected.
               2. there exists two group situation for switchA,switchB,switchC,switchD, only one group 
                  of switchs should be closed at the same time:
                  switchA,switchC: 
                      if closed, the circle is dc->swichTotal->switchC->register->switchA->ground
                  switchB,switchD:
                      if closed, the circle is dc->swichTotal->switchB->register->switchD->ground
             */
            if (totalEnable)
            {
                switchTotal.toggle();
                if (clockwise)
                {
                    switchA.toggle();
                    switchC.toggle();
                }
                else
                {
                    switchB.toggle();
                    switchD.toggle();
                }
            }

            List<ScopeFrame> logicOutTopScope = sim.Watch(logicOutTop);
            List<ScopeFrame> logicOutBottomScope = sim.Watch(logicOutBottom);

            double cycleTime = 1 / dcVoltageSource.frequency;
            double quarterCycleTime = cycleTime / 4;

            int steps = (int)(cycleTime / sim.timeStep);
            sim.analyze();
            for (int x = 1; x <= steps; x++)
            {
                sim.doTick();
            }

            double voltageTopHigh = logicOutTopScope.Max((f) => f.voltage);
            int voltageTopHighNdx = logicOutTopScope.FindIndex((f) => f.voltage == voltageTopHigh);

            double voltageBottomHigh = logicOutBottomScope.Max((f) => f.voltage);
            int voltageBottomHighNdx = logicOutBottomScope.FindIndex((f) => f.voltage == voltageBottomHigh);

            if (totalEnable)
            {
                if (clockwise)
                {
                    TestUtils.Compare(voltageBottomHigh, dcVoltageSource.dutyCycle, 4);
                    TestUtils.Compare(logicOutBottomScope[voltageBottomHighNdx].time, quarterCycleTime, 4);
                }
                else
                {
                    TestUtils.Compare(voltageTopHigh, dcVoltageSource.dutyCycle, 4);
                    TestUtils.Compare(logicOutTopScope[voltageTopHighNdx].time, quarterCycleTime, 4);
                }
            }
            else
            {
                Assert.AreEqual(voltageTopHigh, 0);
                Assert.AreEqual(voltageBottomHigh, 0);
            }

            string a = logicOutTop.GetVoltageString();
            string b = logicOutBottom.GetVoltageString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="reverse">if the control circute is reverse direction.</param>
        [TestCase(true, true)]
        //[TestCase(true, false)]
        //[TestCase(false, true)]
        //[TestCase(false, false)]
        public void SwitchSPSTTest1(bool totalEnable, bool reverse)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.AC);
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            Resistor resistor = sim.Create<Resistor>();

            Ground grndTop = sim.Create<Ground>();

            LogicOutput logicIn = sim.Create<LogicOutput>();
            LogicOutput logicOut = sim.Create<LogicOutput>();

            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, resistor.leadIn);
            sim.Connect(resistor.leadOut, grndTop.leadIn);
            sim.Connect(resistor.leadIn, logicIn.leadIn);
            sim.Connect(resistor.leadOut, logicOut.leadIn);

            switchTotal.toggle();
            sim.analyze();
            //sim.Connect(logicOutTop, 0, resistor, 0);
            //sim.Connect(logicOutBottom, 0, resistor, 1);

            List<ScopeFrame> logicInScope = sim.Watch(logicIn);
            List<ScopeFrame> logicOutScope = sim.Watch(logicOut);

            double cycleTime = 1 / dcVoltageSource.frequency;
            double quarterCycleTime = cycleTime / 4;

            int steps = (int)(cycleTime / sim.timeStep);
            for (int x = 1; x <= steps; x++)
            {
                sim.doTick();
            }

            double voltageInHigh = logicInScope.Max((f) => f.voltage);
            double voltageOutHigh = logicOutScope.Max((f) => f.voltage);
            int voltageInHighNdx = logicInScope.FindIndex((f) => f.voltage == voltageInHigh);
            int voltageOutHighNdx = logicOutScope.FindIndex((f) => f.voltage == voltageInHigh);

            string a = logicIn.GetVoltageString();
            string b = logicOut.GetVoltageString();

            TestUtils.Compare(voltageInHigh, dcVoltageSource.dutyCycle, 4);
            TestUtils.Compare(logicInScope[voltageInHighNdx].time, quarterCycleTime, 4);
        }
    }
}
