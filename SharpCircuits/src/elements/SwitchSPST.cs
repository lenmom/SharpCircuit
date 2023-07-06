namespace SharpCircuit
{
    /// <summary>
    /// 单刀单掷开关
    /// </summary>
    public class SwitchSPST : CircuitElement
    {
        #region Field

        /// <summary>
        /// position 0 == closed, position 1 == open
        /// </summary>
        protected int position { get; private set; }

        protected int posCount = 2;

        #endregion

        #region Property

        public Circuit.Lead leadA { get { return lead0; } }

        public Circuit.Lead leadB { get { return lead1; } }

        public bool IsOpen { get { return position == 1; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Create new instance of <c>SwitchSPST</c>  which is closed state in default.
        /// </summary>
        public SwitchSPST() : this(false)
        {
            // Nothing to do.            
        }

        public SwitchSPST(bool isOpen) : base()
        {
            position = (isOpen) ? 1 : 0;
            posCount = 2;
        }

        #endregion

        #region Public Method

        public virtual void toggle()
        {
            position++;
            if (position >= posCount)
            {
                position = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"> 
        /// 0 or greater than 1, closed, 
        /// 1 , open
        /// </param>
        public virtual void setPosition(int pos)
        {
            position = pos;
            if (position >= posCount)
            {
                position = 0;
            }
        }

        public override void calculateCurrent()
        {
            if (position == 1)
            {
                current = 0;
            }
        }

        public override void stamp(Circuit sim)
        {
            if (position == 0)
            {
                sim.stampVoltageSource(lead_node[0], lead_node[1], voltSource, 0);
            }
        }

        public override int getVoltageSourceCount()
        {
            return (position == 1) ? 0 : 1;
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = string.Empty;
			if(position == 1) {
				arr[1] = "open";
				arr[2] = "Vd = " + getVoltageDText(getVoltageDiff());
			} else {
				arr[1] = "closed";
				arr[2] = "V = " + getVoltageText(lead_volt[0]);
				arr[3] = "I = " + getCurrentDText(current);
			}
		}*/

        public override bool leadsAreConnected(int n1, int n2)
        {
            return position == 0;
        }

        public override bool isWire()
        {
            return true;
        }

        #endregion
    }
}