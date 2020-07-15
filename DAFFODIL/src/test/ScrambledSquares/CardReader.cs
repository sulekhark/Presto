using System;
using System.IO;

namespace ScrambledSquares
{
    class CardReader
    {
        private string fileName;
        public CardReader(string inputFileName)
        {
            this.fileName = inputFileName;
        }
        private PartColor GetColor(string str)
        {
            string s = str.Substring(str.IndexOf("-") + 1);
            return (PartColor)Enum.Parse(typeof(PartColor), s, true);
        }
        private Part GetPart(string str)
        {
            string s = str.Substring(0, str.IndexOf("-"));
            return (Part)Enum.Parse(typeof(Part), s, true);
        }

        private void MakeAllCardInt(int dimension, Card[,] cards)
        {
            using (TextReader scanner = new StreamReader(this.fileName))
            {
                scanner.ReadLine();
                scanner.ReadLine();
                scanner.ReadLine();
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        string line = scanner.ReadLine();
                        Side left, right, top, bottom;
                        string[] seperatrs = { " ", "\t" };
                        string[] splites = null;
                        using (StringReader reader = new StringReader(line))
                        {
                            splites = reader.ReadToEnd().Split(seperatrs, StringSplitOptions.RemoveEmptyEntries);
                            if (splites.Length < 5) throw new FormatException();
                        }
                        top = new Side(GetPart(splites[1]), GetColor(splites[1]));
                        right = new Side(GetPart(splites[2]), GetColor(splites[2]));
                        bottom = new Side(GetPart(splites[3]), GetColor(splites[3]));
                        left = new Side(GetPart(splites[4]), GetColor(splites[4]));

                        cards[i,j] = new Card(splites[0], top, left, bottom, right);
                    }
                }
            }
        }

        public Card[,] MakeAllCard(int dimension)
        {
            Card[,] cards = null;
            try
            {
                cards = new Card[dimension, dimension];
                MakeAllCardInt(dimension, cards);
            }
            catch (Exception e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            return cards;
        }
    }
}
