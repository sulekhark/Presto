using System;

namespace ScrambledSquares
{
    public class Border
    {
        public Side Top { get; protected set; }
        public Side Left { get; protected set; }
        public Side Right { get; protected set; }
        public Side Bottom { get; protected set; }
        public bool CanRotate { get { return Orientation < 4; } }
        public int Orientation { get; protected set; }

        public Border(Side top, Side left, Side bottom, Side right)
        {
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
            this.Top = top;
        }

        public void Rotate()
        {
            if (CanRotate)
            {
                Side tmp = this.Top;
                this.Top = this.Right;
                this.Right = this.Bottom;
                this.Bottom = this.Left;
                this.Left = tmp;
                ++Orientation;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public void RotateFixed()
        {
            Side tmp = this.Top;
            this.Top = this.Right;
            this.Right = this.Bottom;
            this.Bottom = this.Left;
            this.Left = tmp;
        }
        public void ResetOrientation()
        {
            while (Orientation > 0)
            {
                Side tmp = this.Top;
                this.Top = this.Left;
                this.Left = this.Bottom;
                this.Bottom = this.Right;
                this.Right = tmp;
                --Orientation;
            }
        }
        public override string ToString()
        {
            return "  @TOP:" + Top + "  @RIGHT:" + Right
                + "  @BOTTOM:" + Bottom + "  @LEFT:" + Left;
        }
    }
}
