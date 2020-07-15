using System;

namespace ScrambledSquares
{
    public class Side
    {
        public Part Part { get; protected set; }
        public PartColor Color { get; protected set; }
        public int Value
        {
            get { return (int)Part * (int)Color; }
        }

        public Side(string part, string color)
        {
            this.Color = (PartColor)Enum.Parse(typeof(PartColor), color, true);
            this.Part = (Part)Enum.Parse(typeof(Part), part, true);
        }
        public Side(Part part, PartColor color)
        {
            this.Part = part;
            this.Color = color;
        }

        public bool CanJoin(Side side)
        {
            if (side == null)
                return true;
            return this.Value + side.Value == 0;
        }
        public override string ToString()
        {
            return this.Part.ToString() + "-" + this.Color.ToString();
        }
        public Side Clone()
        {
            return new Side(Part, Color);
        }
    }
}
