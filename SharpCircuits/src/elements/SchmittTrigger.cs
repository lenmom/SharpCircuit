using System;

namespace SharpCircuit
{
    /// <summary>
    /// 施密特触发器有两个稳定状态，但与一般触发器不同的是，施密特触发器采用电位触发方式，其状态由输入信号电位维持；
    /// 对于负向递减和正向递增两种不同变化方向的输入信号，施密特触发器有不同的阈值电压。
    /// Contributed by Edward Calver.
    /// </summary>
    public class SchmittTrigger : InvertingSchmittTrigger
    {

        public override void step(Circuit sim)
        {
            double v0 = lead_volt[1];
            double @out;
            if (state)
            {
                // Output is high
                if (lead_volt[0] > upperTrigger)
                {
                    // Input voltage high enough to set output high
                    state = false;
                    @out = 5;
                }
                else
                {
                    @out = 0;
                }
            }
            else
            {
                // Output is low
                if (lead_volt[0] < lowerTrigger)
                {
                    // Input voltage low enough to set output low
                    state = true;
                    @out = 0;
                }
                else
                {
                    @out = 5;
                }
            }
            double maxStep = slewRate * sim.timeStep * 1e9;
            @out = Math.Max(Math.Min(v0 + maxStep, @out), v0 - maxStep);
            sim.updateVoltageSource(0, lead_node[1], voltSource, @out);
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "Schmitt";
		}*/

    }
}