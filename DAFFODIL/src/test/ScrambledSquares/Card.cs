namespace ScrambledSquares
{
    public class Card : Border
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Position { get; set; }
        public bool Used { get; set; }
        public string Name { get { return "Card" + name; } }

        private string name;

        public Card(string name, Side top, Side left, Side bottom, Side right)
            : base(top, left, bottom, right)
        {
            this.name = name;
            this.Row = this.Column = this.Position = -1;
        }

        public bool IsCompatible(Card left, Card top)
        {
            if (left == null && top != null)
            {
                return this.Top.CanJoin(top.Bottom);
            }
            else if (left != null && top == null)
            {
                return this.Left.CanJoin(left.Right);
            }
            else if (left == null && top == null)
            {
                return true;
            }
            return this.Left.CanJoin(left.Right) && this.Top.CanJoin(top.Bottom);
        }
        public override string ToString()
        {
            return this.Name;
        }
        public string ToString(bool fullDetails)
        {
            return (!fullDetails) ?
                ToString() :
                this.Name + ",\tAfter " + this.Orientation + " rotation" + ",\t" + base.ToString();
        }
        public Card Clone()
        {
            Card c = new Card(string.Copy(name), Top.Clone(), Left.Clone(), Bottom.Clone(), Right.Clone());
            c.Orientation = this.Orientation;
            c.Position = this.Position;
            c.Row = this.Row;
            c.Column = this.Column;
            c.Used = this.Used;
            return c;
        }
    }
}
