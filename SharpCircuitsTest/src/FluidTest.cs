using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NUnit.Framework;

using SharpCircuit;

using static SharpCircuit.Circuit;

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
        public void SwitchSPSTRegisterTest(bool totalEnable, bool clockwise)
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
        /// 变阻器示例
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="clockwise">if the control circute is reverse direction.</param>
        [TestCase(true, true, 0.005)]
        [TestCase(true, false, 0.200)]
        [TestCase(false, true, 0.400)]
        [TestCase(false, false, 0.5)]
        public void SwitchSPSTPotentiometerTest(bool totalEnable, bool clockwise, double resisterPosition)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            SwitchSPST switchA = sim.Create<SwitchSPST>(true);
            SwitchSPST switchB = sim.Create<SwitchSPST>(true);
            SwitchSPST switchC = sim.Create<SwitchSPST>(true);
            SwitchSPST switchD = sim.Create<SwitchSPST>(true);

            Potentiometer resistor = sim.Create<Potentiometer>();
            resistor.position = resisterPosition;

            Ground grndTop = sim.Create<Ground>();
            Ground grndBottom = sim.Create<Ground>();

            LogicOutput logicOutTop = sim.Create<LogicOutput>();
            LogicOutput logicOutBottom = sim.Create<LogicOutput>();

            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, switchB.leadA);
            sim.Connect(switchTotal.leadB, switchC.leadA);

            sim.Connect(switchA.leadB, grndTop.leadIn);
            sim.Connect(switchA.leadA, resistor.leadVoltage);

            sim.Connect(switchB.leadB, resistor.leadVoltage);

            sim.Connect(switchC.leadB, resistor.leadIn);

            sim.Connect(switchD.leadB, grndBottom.leadIn);
            sim.Connect(switchD.leadA, resistor.leadIn);

            sim.Connect(logicOutTop.leadIn, resistor.leadVoltage);
            sim.Connect(logicOutBottom.leadIn, resistor.leadIn);

            /*
               1. if switchTotal closed, then the whole circute is connected; otherwise, disconnected.
               2. there exists two group situation for switchA,switchB,switchC,switchD, only one group 
                  of switchs should be closed at the same time:
                  switchA,switchC: 
                      if closed, the circle is dc->swichTotal->switchC->register->switchA->ground
                  switchB,switchD:
                      if closed, the circle is dc->swichTotal->switchB->register->switchD->ground
               3. change the position would change to whole resistance value in the circute.
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


            sim.analyze();
            sim.doTick();

            double current1 = resistor.current2;

            resistor.position = +0.01;

            sim.analyze();
            sim.doTick();

            double current2 = resistor.current2;

            if (totalEnable)
            {
                Assert.Less(current1, current2);
            }
            else
            {
                Assert.AreEqual(current1, current2);
                Assert.AreEqual(current1, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="reverse">if the control circute is reverse direction.</param>
        [TestCase(true, 5, 0.05)]
        [TestCase(false, 0, 0)]
        public void SwitchSPSTResistorDCTest(bool switchEnable, double voltVal, double currentVal)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.DC);
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

            if (switchEnable)
            {
                switchTotal.toggle();
            }

            sim.analyze();
            sim.doTicks(100);

            Assert.AreEqual(voltVal, logicIn.getLeadVoltage(0));
            Assert.AreEqual(0, logicOut.getLeadVoltage(0));
            Assert.AreEqual(currentVal, resistor.getCurrent());
        }



        /// <summary>
        /// 安全保护回路
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="clockwise">if the control circute is reverse direction.</param>
        /// //leadA + ;leadb -;leadin +;leadout -;
        [TestCase(true, true)]
        //[TestCase(true, false)]
        //[TestCase(false, true)]
        //[TestCase(false, false)]
        public void SwitchSPSTPotentiometerTestPIC1(bool totalEnable, bool clockwise)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            dcVoltageSource.maxVoltage = 240;

            // true:  the switch is open;
            // false: the switch is closed.
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            // the main circute control switch, default state, 13/24, 
            // clockwise:  
            //      true:   swich1 & swich3 close, and swich2 & switch4 open.
            //      false:  swich2 & switch4 close, and swich1 & swich3 open.
            SwitchSPST switch1 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch2 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch3 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch4 = sim.Create<SwitchSPST>(true);

            /* control circute:
             *     switch5 & switch8 closed represents the control cirtute take effect.
            */
            SwitchSPST switch5 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch6 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch7 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch8 = sim.Create<SwitchSPST>(true);

            Resistor resistor1 = sim.Create<Resistor>();
            Resistor resistor2 = sim.Create<Resistor>();
            Resistor resistor3 = sim.Create<Resistor>();
            Resistor resistor4 = sim.Create<Resistor>();
            Resistor resistor5 = sim.Create<Resistor>();
            Resistor resistor6 = sim.Create<Resistor>();
            Resistor resistor7 = sim.Create<Resistor>();
            Resistor resistor8 = sim.Create<Resistor>();
            Resistor resistor9 = sim.Create<Resistor>();
            Resistor resistor10 = sim.Create<Resistor>();
            Resistor resistor11 = sim.Create<Resistor>();
            Resistor resistor12 = sim.Create<Resistor>();
            Resistor resistor13 = sim.Create<Resistor>();

            Ground grnd1 = sim.Create<Ground>();
            Ground grnd2 = sim.Create<Ground>();
            Ground grnd3 = sim.Create<Ground>();
            Ground grnd4 = sim.Create<Ground>();
            Ground grnd5 = sim.Create<Ground>();

            //总电源连接R4电路
            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, resistor4.leadIn);
            //R4电阻
            sim.Connect(resistor4.leadOut, switch2.leadA);
            sim.Connect(resistor4.leadOut, switch3.leadA);

            //Key2
            sim.Connect(switch2.leadB, resistor1.leadIn);
            //key3
            sim.Connect(switch3.leadB, resistor2.leadIn);
            //R1
            sim.Connect(resistor1.leadOut, resistor10.leadIn);
            sim.Connect(resistor1.leadIn, switch1.leadA);
            //R2
            sim.Connect(resistor2.leadIn, switch4.leadA);
            //R10
            sim.Connect(resistor10.leadOut, resistor2.leadOut);

            //Key1
            sim.Connect(switch1.leadB, resistor3.leadIn);
            //R3
            sim.Connect(resistor3.leadOut, grnd1.leadIn);
            //KEY4
            sim.Connect(switch4.leadB, resistor5.leadIn);
            //R5
            sim.Connect(resistor5.leadOut, grnd2.leadIn);


            //总电源连接R13电路
            sim.Connect(switchTotal.leadB, resistor13.leadIn);
            //R13
            sim.Connect(resistor13.leadOut, switch8.leadA);
            //key8
            sim.Connect(switch8.leadB, resistor12.leadIn);
            //R12
            sim.Connect(resistor12.leadOut, resistor7.leadIn);
            sim.Connect(resistor12.leadIn, switch7.leadA);
            //key7
            sim.Connect(switch7.leadB, resistor11.leadIn);
            //R11
            sim.Connect(resistor11.leadOut, grnd5.leadIn);
            //R7
            sim.Connect(resistor7.leadOut, switch5.leadA);
            //key5
            sim.Connect(switch5.leadB, resistor8.leadIn);
            //R8
            sim.Connect(resistor8.leadOut, resistor6.leadIn);
            sim.Connect(resistor8.leadIn, switch6.leadA);
            //key6
            sim.Connect(switch6.leadB, resistor9.leadIn);
            //R9
            sim.Connect(resistor9.leadOut, grnd4.leadIn);
            //R6
            sim.Connect(resistor6.leadOut, grnd3.leadIn);

            /*
               1. if switchTotal closed, then the whole circute is connected; otherwise, disconnected.
               2. there exists two group situation for switchA,switchB,switchC,switchD, only one group 
                  of switchs should be closed at the same time:
                  switchA,switchC: 
                      if closed, the circle is dc->swichTotal->switchC->register->switchA->ground
                  switchB,switchD:
                      if closed, the circle is dc->swichTotal->switchB->register->switchD->ground
               3. change the position would change to whole resistance value in the circute.
             */
            if (totalEnable)
            {
                switchTotal.toggle();
                if (clockwise)
                {
                    switch1.toggle();
                    switch3.toggle();
                }
                else
                {
                    switch5.toggle();
                    switch8.toggle();

                    switch2.toggle();
                    switch4.toggle();
                }
            }


            sim.analyze();
            sim.doTick();

            double current10 = resistor10.getCurrent();
            double current6 = resistor6.getCurrent();

            if (totalEnable)
            {
                if (clockwise)
                {
                    Assert.Greater(current10 * -1, 0);
                    Assert.AreEqual(current6, 0);
                }
                else
                {
                    Assert.Greater(resistor10.getCurrent(), 0);
                    Assert.Greater(resistor6.getCurrent(), 0);
                }

            }
            else
            {
                Assert.AreEqual(current10, 0);
                Assert.AreEqual(current6, 0);
            }
        }

        /// <summary>
        /// 顺序动作回路
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="clockwise">if the control circute is reverse direction.</param>
        /// //leadA + ;leadb -;leadin +;leadout -;
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void SwitchSPSTPotentiometerTestPIC2(bool totalEnable, bool clockwise)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            dcVoltageSource.maxVoltage = 240;

            // true:  the switch is open;
            // false: the switch is closed.
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            // the main circute control switch, default state, 1113/1214, 
            // clockwise:  
            //      true:   swich13 & swich15 close, and swich14 & switch16open.
            //      false:  swich14& switch16 close, and swich13 & swich15 open.
            SwitchSPST switch13 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch14 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch15 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch16 = sim.Create<SwitchSPST>(true);

            /* control circute:
             *     switch12 || switch17 closed represents the control cirtute take effect.
            */
            SwitchSPST switch12 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch17 = sim.Create<SwitchSPST>(true);

            SwitchSPST switch11 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch18 = sim.Create<SwitchSPST>(true);

            Resistor resistor17 = sim.Create<Resistor>();
            Resistor resistor18 = sim.Create<Resistor>();
            Resistor resistor19 = sim.Create<Resistor>();
            Resistor resistor20 = sim.Create<Resistor>();
            Resistor resistor21 = sim.Create<Resistor>();
            Resistor resistor22 = sim.Create<Resistor>();
            Resistor resistor23 = sim.Create<Resistor>();
            Resistor resistor24 = sim.Create<Resistor>();
            Resistor resistor25 = sim.Create<Resistor>();
            Resistor resistor26 = sim.Create<Resistor>();
            Resistor resistor27 = sim.Create<Resistor>();
            Resistor resistor28 = sim.Create<Resistor>();
            Resistor resistor29 = sim.Create<Resistor>();
            Resistor resistor30 = sim.Create<Resistor>();

            Ground grnd1 = sim.Create<Ground>();
            Ground grnd2 = sim.Create<Ground>();
            Ground grnd3 = sim.Create<Ground>();
            Ground grnd4 = sim.Create<Ground>();
            Ground grnd5 = sim.Create<Ground>();
            Ground grnd6 = sim.Create<Ground>();

            //总电源连接R23电路
            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, resistor23.leadIn);
            //R23
            sim.Connect(resistor23.leadOut, switch15.leadA);
            sim.Connect(resistor23.leadOut, switch14.leadA);
            //Key15
            sim.Connect(switch15.leadB, resistor26.leadIn);
            //Key14
            sim.Connect(switch14.leadB, resistor25.leadIn);
            //R26
            sim.Connect(resistor26.leadOut, resistor17.leadIn);
            sim.Connect(resistor26.leadIn, switch16.leadA);
            //R25
            sim.Connect(resistor25.leadOut, resistor17.leadOut);
            sim.Connect(resistor25.leadIn, switch13.leadA);
            //Key16
            sim.Connect(switch16.leadB, resistor24.leadIn);
            //R24
            sim.Connect(resistor24.leadOut, grnd3.leadIn);
            //KEY13
            sim.Connect(switch13.leadB, resistor22.leadIn);
            //R22
            sim.Connect(resistor22.leadOut, grnd4.leadIn);


            //总电源连接R30电路
            sim.Connect(switchTotal.leadB, resistor30.leadIn);
            //R30
            sim.Connect(resistor30.leadOut, switch18.leadA);
            //key18
            sim.Connect(switch18.leadB, resistor29.leadIn);
            //R29
            sim.Connect(resistor29.leadOut, resistor27.leadIn);
            sim.Connect(resistor29.leadIn, switch17.leadA);
            //key17
            sim.Connect(switch17.leadB, resistor28.leadIn);
            //R28
            sim.Connect(resistor28.leadOut, grnd1.leadIn);
            //R27
            sim.Connect(resistor27.leadOut, grnd2.leadIn);

            //总电源连接R20电路
            sim.Connect(switchTotal.leadB, resistor20.leadIn);
            //R20
            sim.Connect(resistor20.leadOut, switch12.leadA);
            //key12
            sim.Connect(switch12.leadB, resistor19.leadIn);
            //R19
            sim.Connect(resistor19.leadOut, resistor21.leadIn);
            sim.Connect(resistor19.leadIn, switch11.leadA);
            //key11
            sim.Connect(switch11.leadB, resistor18.leadIn);
            //R18
            sim.Connect(resistor18.leadOut, grnd6.leadIn);
            //R21
            sim.Connect(resistor21.leadOut, grnd5.leadIn);


            /*
               1. if switchTotal closed, then the whole circute is connected; otherwise, disconnected.
               2. there exists two group situation for switchA,switchB,switchC,switchD, only one group 
                  of switchs should be closed at the same time:
                  switchA,switchC: 
                      if closed, the circle is dc->swichTotal->switchC->register->switchA->ground
                  switchB,switchD:
                      if closed, the circle is dc->swichTotal->switchB->register->switchD->ground
               3. change the position would change to whole resistance value in the circute.
             */
            if (totalEnable)
            {
                switchTotal.toggle();
                if (clockwise)
                {
                    switch14.toggle();
                    switch16.toggle();

                    switch18.toggle();
                }
                else
                {

                    switch12.toggle();

                    switch13.toggle();
                    switch15.toggle();

                }
            }


            sim.analyze();
            sim.doTick();

            double current17 = resistor17.getCurrent();
            double current21 = resistor21.getCurrent();
            double current27 = resistor27.getCurrent();

            if (totalEnable)
            {
                if (clockwise)
                {
                    Assert.Greater(current17 * -1, 0);
                    Assert.AreEqual(current21, 0);
                    Assert.Greater(current27, 0);
                }
                else
                {
                    Assert.Greater(current17, 0);
                    Assert.Greater(current21, 0);
                    Assert.AreEqual(current27, 0);
                }

            }
            else
            {
                Assert.AreEqual(current17, 0);
                Assert.AreEqual(current21, 0);
                Assert.AreEqual(current27, 0);
            }
        }

        /// <summary>
        /// 去毛刺工位气动回路
        /// </summary>
        /// <param name="totalEnable">if the total switch is closed.</param>
        /// <param name="clockwise">if the control circute is reverse direction.</param>
        /// //leadA + ;leadb -;leadin +;leadout -;
        [TestCase(true, true, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        public void SwitchSPSTPotentiometerTestPIC3(bool totalEnable, bool topClockwise, bool downClockwise)
        {
            Circuit sim = new Circuit();

            VoltageInput dcVoltageSource = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            dcVoltageSource.maxVoltage = 240;

            // true:  the switch is open;
            // false: the switch is closed.
            SwitchSPST switchTotal = sim.Create<SwitchSPST>(true);

            // top, 

            SwitchSPST switch19 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch20 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch21 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch22 = sim.Create<SwitchSPST>(true);

            /* control circute:
             *     down.
            */
            SwitchSPST switch24 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch23 = sim.Create<SwitchSPST>(true);

            SwitchSPST switch10 = sim.Create<SwitchSPST>(true);
            SwitchSPST switch9 = sim.Create<SwitchSPST>(true);

            Potentiometer resistor14 = sim.Create<Potentiometer>();
            Potentiometer resistor15 = sim.Create<Potentiometer>();
            Potentiometer resistor16 = sim.Create<Potentiometer>();
            Potentiometer resistor31 = sim.Create<Potentiometer>();

            Resistor resistor32 = sim.Create<Resistor>();
            Resistor resistor33 = sim.Create<Resistor>();
            Resistor resistor34 = sim.Create<Resistor>();
            Resistor resistor35 = sim.Create<Resistor>();
            Resistor resistor36 = sim.Create<Resistor>();

            Resistor resistor38 = sim.Create<Resistor>();
            Resistor resistor39 = sim.Create<Resistor>();
            Resistor resistor40 = sim.Create<Resistor>();
            Resistor resistor42 = sim.Create<Resistor>();
            Resistor resistor43 = sim.Create<Resistor>();

            Resistor resistor37 = sim.Create<Resistor>();
            Resistor resistor41 = sim.Create<Resistor>();

            Ground grnd1 = sim.Create<Ground>();
            Ground grnd2 = sim.Create<Ground>();
            Ground grnd3 = sim.Create<Ground>();
            Ground grnd4 = sim.Create<Ground>();


            //总电源连接R35电路
            sim.Connect(dcVoltageSource.leadPos, switchTotal.leadA);
            sim.Connect(switchTotal.leadB, resistor35.leadIn);
            //R35
            sim.Connect(resistor35.leadOut, switch20.leadA);
            sim.Connect(resistor35.leadOut, switch21.leadA);
            //Key20
            sim.Connect(switch20.leadB, resistor32.leadIn);
            //Key21
            sim.Connect(switch21.leadB, resistor33.leadIn);
            //R32
            sim.Connect(resistor32.leadOut, resistor15.leadIn);
            sim.Connect(resistor32.leadOut, switch19.leadA);
            //R33
            sim.Connect(resistor33.leadOut, resistor14.leadIn);
            sim.Connect(resistor33.leadOut, switch22.leadA);
            //R15
            sim.Connect(resistor15.leadVoltage, resistor41.leadIn);
            //R14
            sim.Connect(resistor14.leadVoltage, resistor41.leadOut);
            //KEY19
            sim.Connect(switch19.leadB, resistor34.leadIn);
            //R34
            sim.Connect(resistor34.leadOut, grnd1.leadIn);
            //KEY22
            sim.Connect(switch22.leadB, resistor36.leadIn);
            //R36
            sim.Connect(resistor36.leadOut, grnd2.leadIn);


            //总电源连接R39电路
            sim.Connect(switchTotal.leadB, resistor39.leadIn);
            //R39
            sim.Connect(resistor39.leadOut, switch23.leadA);
            sim.Connect(resistor39.leadOut, switch10.leadA);
            //key23
            sim.Connect(switch23.leadB, resistor43.leadIn);
            //key10
            sim.Connect(switch10.leadB, resistor42.leadIn);
            //R43
            sim.Connect(resistor43.leadOut, resistor16.leadIn);
            sim.Connect(resistor43.leadOut, switch24.leadA);
            //R42
            sim.Connect(resistor42.leadOut, resistor31.leadIn);
            sim.Connect(resistor42.leadOut, switch9.leadA);
            //R16
            sim.Connect(resistor16.leadVoltage, resistor37.leadIn);
            //R31
            sim.Connect(resistor31.leadVoltage, resistor37.leadOut);
            //key24
            sim.Connect(switch24.leadB, resistor40.leadIn);
            //R40
            sim.Connect(resistor40.leadOut, grnd3.leadIn);
            //key9
            sim.Connect(switch9.leadB, resistor38.leadIn);
            //R38
            sim.Connect(resistor38.leadOut, grnd4.leadIn);


            if (totalEnable)
            {
                switchTotal.toggle();
                if (topClockwise)
                {
                    switch20.toggle();
                    switch22.toggle();
                }
                else
                {
                    switch19.toggle();
                    switch21.toggle();
                }
                if (downClockwise)
                {
                    switch9.toggle();
                    switch23.toggle();
                }
                else
                {
                    switch10.toggle();
                    switch24.toggle();
                }
            }


            sim.analyze();
            sim.doTick();

            double current41 = resistor41.getCurrent();
            double current37 = resistor37.getCurrent();

            if (totalEnable)
            {
                if (topClockwise)
                {
                    Assert.Greater(current41, 0);
                }
                else
                {
                    Assert.Greater(current41 * -1, 0);
                }
                if (downClockwise)
                {
                    Assert.Greater(current37, 0);
                }
                else
                {
                    Assert.Greater(current37 * -1, 0);
                }
            }
            else
            {
                Assert.AreEqual(current37, 0);
                Assert.AreEqual(current41, 0);
            }
        }


    }
}
