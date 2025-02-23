﻿using System;
using System.Text;

namespace SharpCircuit
{
    internal class Program
    {

        public static double Round(double val, int places)
        {
            if (places < 0)
            {
                throw new ArgumentException("places");
            }

            return Math.Round(val - (0.5 / Math.Pow(10, places)), places);
        }

        private static void Main(string[] args)
        {

            Circuit sim = new Circuit();

            VoltageInput volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            Resistor res0 = sim.Create<Resistor>();
            Ground ground0 = sim.Create<Ground>();

            sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(res0.leadOut, ground0.leadIn);

            for (int x = 1; x <= 100; x++)
            {
                sim.doTick();
                // Ohm's Law
                Debug.Log(res0.getVoltageDelta(), res0.resistance * res0.getCurrent()); // V = I x R
                Debug.Log(res0.getCurrent(), res0.getVoltageDelta() / res0.resistance); // I = V / R
                Debug.Log(res0.resistance, res0.getVoltageDelta() / res0.getCurrent()); // R = V / I
            }

            Console.WriteLine("program complete");
            Console.ReadLine();
        }

    }
}

public static class Debug
{

    public static void Log(params object[] objs)
    {
        StringBuilder sb = new StringBuilder();
        foreach (object o in objs)
        {
            sb.Append(o.ToString()).Append(" ");
        }

        Console.WriteLine(sb.ToString());
        System.Diagnostics.Debug.WriteLine(sb.ToString());
    }

    public static void LogF(string format, params object[] objs)
    {
        Console.WriteLine(string.Format(format, objs));
    }

}