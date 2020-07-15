using System;

namespace ScrambledSquares
{
    class DraftSpace
    {
        private Card[,] cards;
        private int dim;

        public Card[,] Cards { get { return Clone(); } }

        public DraftSpace(int dim)
        {
            this.dim = dim;
            cards = new Card[dim, dim];
        }

        public bool TryAdd(Card crd, int pos)
        {
            Card l = GetLeftCard(pos), t = GetTopCard(pos);
            int r = GetRow(pos), c = GetColumn(pos);

            if (crd.IsCompatible(l, t))
            {
                cards[r, c] = crd.Clone();
                cards[r, c].Position = pos;
                return true;
            }
            return false;
        }
        public void Remove(int pos)
        {
            int r = GetRow(pos), c = GetColumn(pos);
            cards[r, c] = null;
        }
        public void Clear()
        {
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    cards[i, j] = null;
                }
            }
        }

        private Card[,] Clone()
        {
            Card[,] tmp = new Card[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    tmp[i, j] = cards[i, j].Clone();
                }
            }
            return tmp;
        }
        private int GetRow(int pos)
        {
            return pos / dim;
        }
        private int GetColumn(int pos)
        {
            return pos % dim;
        }
        private Card GetLeftCard(int pos)
        {
            int r, c;
            if (pos >= dim * dim)
            {
                return null;
            }
            r = GetRow(pos);
            c = GetColumn(pos);
            try
            {
                return cards[r, c - 1];
            }
            catch (Exception)
            {
                return null;
            }
        }
        private Card GetTopCard(int pos)
        {
            int r, c;
            if (pos >= dim * dim)
            {
                return null;
            }
            r = GetRow(pos);
            c = GetColumn(pos);
            try
            {
                return cards[r - 1, c];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
