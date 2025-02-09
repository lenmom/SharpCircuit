using System;

namespace SharpCircuit
{
    /// <summary>
    /// 半导体放电管,是一种过压保护器件，是利用晶闸管原理制成的，依靠PN结的击穿电流触发器件导通放电，
    /// 可以流SPG强效应玻璃放电管是一种靠內部微间隙放电的一种保护器件，在电极微间隙之间充有稳定的隋性气体，
    /// 并采用玻璃壳和杜镁丝头在高温下烧结密封而形成的.
    /// </summary>
    public class SparkGap : CircuitElement
    {

        public Circuit.Lead leadIn { get { return lead0; } }
        public Circuit.Lead leadOut { get { return lead1; } }

        /// <summary>
        /// On resistance (ohms)
        /// </summary>
        public double onresistance { get; set; }

        /// <summary>
        /// Off resistance (ohms)
        /// </summary>
        public double offresistance { get; set; }

        /// <summary>
        /// Breakdown voltage
        /// </summary>
        public double breakdown { get; set; }

        /// <summary>
        /// Holding current (A)
        /// </summary>
        public double holdcurrent { get; set; }

        private double resistance;
        private bool state;

        public SparkGap() : base()
        {
            offresistance = 1E9;
            onresistance = 1E3;
            breakdown = 1E3;
            holdcurrent = 0.001;
            state = false;
        }

        public override bool nonLinear() { return true; }

        public override void calculateCurrent()
        {
            double vd = lead_volt[0] - lead_volt[1];
            current = vd / resistance;
        }

        public override void reset()
        {
            base.reset();
            state = false;
        }

        public override void beginStep(Circuit sim)
        {
            if (Math.Abs(current) < holdcurrent)
            {
                state = false;
            }

            double vd = lead_volt[0] - lead_volt[1];
            if (Math.Abs(vd) > breakdown)
            {
                state = true;
            }
        }

        public override void step(Circuit sim)
        {
            resistance = (state) ? onresistance : offresistance;
            sim.stampResistor(lead_node[0], lead_node[1], resistance);
        }

        public override void stamp(Circuit sim)
        {
            sim.stampNonLinear(lead_node[0]);
            sim.stampNonLinear(lead_node[1]);
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "spark gap";
			getBasicInfo(arr);
			arr[3] = state ? "on" : "off";
			arr[4] = "Ron = " + getUnitText(onresistance, Circuit.ohmString);
			arr[5] = "Roff = " + getUnitText(offresistance, Circuit.ohmString);
			arr[6] = "Vbreakdown = " + getUnitText(breakdown, "V");
		}*/

    }
}